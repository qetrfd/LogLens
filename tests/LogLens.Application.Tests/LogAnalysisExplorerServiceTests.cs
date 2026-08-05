using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogAnalysisExplorerServiceTests
{
    [Fact]
    public void QueryGroupsFiltersAndSortsByFrequency()
    {
        DateTimeOffset timestamp = new(
            2026,
            8,
            4,
            22,
            0,
            0,
            TimeSpan.Zero);

        LogGroupSummary[] groups =
        [
            CreateGroup(
                "api-timeout",
                "Gateway timeout",
                LogLevel.Error,
                8,
                timestamp,
                "API",
                "TimeoutException",
                504),

            CreateGroup(
                "api-connection",
                "Connection refused",
                LogLevel.Error,
                3,
                timestamp.AddMinutes(1),
                "API",
                "SocketException",
                503),

            CreateGroup(
                "database-critical",
                "Database unavailable",
                LogLevel.Critical,
                20,
                timestamp.AddMinutes(2),
                "Database",
                "DatabaseException",
                500)
        ];

        LogAnalysisExplorerService service =
            new();

        IReadOnlyList<LogGroupSummary> result =
            service.QueryGroups(
                groups,
                new LogGroupQueryOptions(
                    "api",
                    LogLevel.Error,
                    LogGroupSortOrder.Frequency));

        Assert.Collection(
            result,
            first =>
                Assert.Equal(
                    "Gateway timeout",
                    first.RepresentativeMessage),
            second =>
                Assert.Equal(
                    "Connection refused",
                    second.RepresentativeMessage));
    }

    [Fact]
    public void QueryGroupsSearchesSamples()
    {
        DateTimeOffset timestamp = new(
            2026,
            8,
            4,
            22,
            0,
            0,
            TimeSpan.Zero);

        LogGroupSummary group = CreateGroup(
            "request-failure",
            "Request failed",
            LogLevel.Error,
            1,
            timestamp,
            "API",
            "InvalidOperationException",
            500,
            "Request failed correlationId=req-900");

        LogAnalysisExplorerService service =
            new();

        LogGroupSummary result =
            Assert.Single(
                service.QueryGroups(
                    [group],
                    new LogGroupQueryOptions(
                        "req-900")));

        Assert.Equal(
            group.Fingerprint.Value,
            result.Fingerprint.Value);
    }

    [Fact]
    public void QueryDiagnosesFiltersAndSortsByConfidence()
    {
        DateTimeOffset timestamp = new(
            2026,
            8,
            4,
            22,
            0,
            0,
            TimeSpan.Zero);

        IncidentDiagnosis[] diagnoses =
        [
            CreateDiagnosis(
                "database-connection",
                "Database connection failure",
                IncidentPriority.High,
                82,
                timestamp),

            CreateDiagnosis(
                "database-unavailable",
                "Database unavailable",
                IncidentPriority.High,
                96,
                timestamp.AddMinutes(1)),

            CreateDiagnosis(
                "slow-request",
                "Slow request",
                IncidentPriority.Medium,
                99,
                timestamp.AddMinutes(2))
        ];

        LogAnalysisExplorerService service =
            new();

        IReadOnlyList<IncidentDiagnosis> result =
            service.QueryDiagnoses(
                diagnoses,
                new IncidentDiagnosisQueryOptions(
                    "database",
                    IncidentPriority.High,
                    IncidentDiagnosisSortOrder.Confidence));

        Assert.Collection(
            result,
            first =>
                Assert.Equal(
                    "Database unavailable",
                    first.Title),
            second =>
                Assert.Equal(
                    "Database connection failure",
                    second.Title));
    }

    [Fact]
    public void QueryDiagnosesSearchesActions()
    {
        IncidentDiagnosis diagnosis =
            new(
                "connection-failure",
                "Connection failure",
                "The destination service is unavailable.",
                IncidentPriority.High,
                90,
                "connection-fingerprint",
                [
                    new DiagnosticEvidence(
                        "connection",
                        "Connection",
                        "Refused")
                ],
                [
                    "Restart the destination service.",
                    "Verify network connectivity."
                ],
                DateTimeOffset.UtcNow);

        LogAnalysisExplorerService service =
            new();

        IncidentDiagnosis result =
            Assert.Single(
                service.QueryDiagnoses(
                    [diagnosis],
                    new IncidentDiagnosisQueryOptions(
                        "network connectivity")));

        Assert.Equal(
            diagnosis.RuleId,
            result.RuleId);
    }

    private static LogGroupSummary CreateGroup(
        string fingerprint,
        string message,
        LogLevel level,
        long occurrenceCount,
        DateTimeOffset timestamp,
        string service,
        string exceptionType,
        int statusCode,
        string? sampleMessage = null)
    {
        Guid sourceId =
            Guid.NewGuid();

        LogGroupSample sample = new(
            sourceId,
            1,
            timestamp,
            level,
            sampleMessage ?? message,
            service,
            exceptionType,
            statusCode);

        return new LogGroupSummary(
            new LogFingerprint(
                fingerprint,
                message.ToLowerInvariant()),
            occurrenceCount,
            timestamp,
            timestamp.AddMinutes(1),
            level,
            message,
            [service],
            [exceptionType],
            [statusCode],
            [sample]);
    }

    private static IncidentDiagnosis CreateDiagnosis(
        string ruleId,
        string title,
        IncidentPriority priority,
        double confidence,
        DateTimeOffset detectedAt)
    {
        return new IncidentDiagnosis(
            ruleId,
            title,
            $"{title} summary",
            priority,
            confidence,
            $"{ruleId}-fingerprint",
            [
                new DiagnosticEvidence(
                    "test-evidence",
                    "Evidence",
                    title)
            ],
            [
                $"Review {title}."
            ],
            detectedAt);
    }
}