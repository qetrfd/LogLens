using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class IncidentDiagnosticServiceTests
{
    [Fact]
    public void DiagnoseOrdersDiagnosesByPriority()
    {
        DateTimeOffset analyzedAt = new(
            2026,
            8,
            4,
            21,
            30,
            0,
            TimeSpan.Zero);

        LogGroupSummary mediumGroup =
            CreateGroup(
                "medium-fingerprint",
                "Request timeout",
                LogLevel.Warning);

        LogGroupSummary criticalGroup =
            CreateGroup(
                "critical-fingerprint",
                "Database unavailable",
                LogLevel.Critical);

        LogGroupingResult grouping =
            CreateGrouping(
                mediumGroup,
                criticalGroup);

        IncidentDiagnosticService service = new(
        [
            new FakeRule(
                "medium-rule",
                "medium-fingerprint",
                IncidentPriority.Medium,
                95,
                200),

            new FakeRule(
                "critical-rule",
                "critical-fingerprint",
                IncidentPriority.Critical,
                80,
                100)
        ]);

        IncidentDiagnosticResult result =
            service.Diagnose(
                grouping,
                analyzedAt);

        Assert.Equal(2, result.DiagnosisCount);
        Assert.Equal(2, result.DiagnosedGroupCount);
        Assert.Equal(1, result.ImmediateAttentionCount);    
        Assert.True(result.HasCriticalIncidents);
        Assert.Equal(analyzedAt, result.AnalyzedAt);

        Assert.Equal(
            IncidentPriority.Critical,
            result.Diagnoses[0].Priority);

        Assert.Equal(
            IncidentPriority.Medium,
            result.Diagnoses[1].Priority);

        Assert.Equal(
            1,
            result.PriorityCounts[
                IncidentPriority.Critical]);

        Assert.Equal(
            1,
            result.PriorityCounts[
                IncidentPriority.Medium]);
    }

    [Fact]
    public void DiagnoseIgnoresDuplicateRuleAndFingerprint()
    {
        LogGroupSummary firstGroup =
            CreateGroup(
                "shared-fingerprint",
                "Database failed",
                LogLevel.Error);

        LogGroupSummary secondGroup =
            CreateGroup(
                "shared-fingerprint",
                "Database failed again",
                LogLevel.Error);

        LogGroupingResult grouping =
            CreateGrouping(
                firstGroup,
                secondGroup);

        IncidentDiagnosticService service = new(
        [
            new FakeRule(
                "shared-rule",
                "shared-fingerprint",
                IncidentPriority.High,
                90,
                100)
        ]);

        IncidentDiagnosticResult result =
            service.Diagnose(grouping);

        Assert.Single(result.Diagnoses);
        Assert.Equal(1, result.DiagnosedGroupCount);
    }

    [Fact]
    public void DiagnoseReturnsEmptyResultForEmptyGrouping()
    {
        LogGroupingResult grouping = new(
            totalEntries: 0,
            groupedEntries: 0,
            excludedEntries: 0,
            groups: [],
            completedAt:
                DateTimeOffset.UtcNow);

        IncidentDiagnosticService service = new(
        [
            new FakeRule(
                "unused-rule",
                "unused-fingerprint",
                IncidentPriority.Low,
                50,
                100)
        ]);

        IncidentDiagnosticResult result =
            service.Diagnose(grouping);

        Assert.Equal(0, result.TotalGroups);
        Assert.Equal(0, result.DiagnosedGroupCount);
        Assert.Equal(0, result.DiagnosisCount);
        Assert.Equal(0, result.ImmediateAttentionCount);
        Assert.False(result.HasCriticalIncidents);
        Assert.Empty(result.Diagnoses);
        Assert.Empty(result.PriorityCounts);
    }

    [Fact]
    public void ConstructorRejectsDuplicateRuleIdentifiers()
    {
        Assert.Throws<ArgumentException>(() =>
            new IncidentDiagnosticService(
            [
                new FakeRule(
                    "duplicate-rule",
                    "first",
                    IncidentPriority.Low,
                    50,
                    100),

                new FakeRule(
                    "DUPLICATE-RULE",
                    "second",
                    IncidentPriority.High,
                    90,
                    200)
            ]));
    }

    [Fact]
    public void ConstructorRejectsEmptyRuleCollection()
    {
        Assert.Throws<ArgumentException>(() =>
            new IncidentDiagnosticService([]));
    }

    private static LogGroupingResult CreateGrouping(
        params LogGroupSummary[] groups)
    {
        long totalEntries =
            groups.Sum(
                group => group.OccurrenceCount);

        return new LogGroupingResult(
            totalEntries,
            totalEntries,
            0,
            groups,
            DateTimeOffset.UtcNow);
    }

    private static LogGroupSummary CreateGroup(
        string fingerprintValue,
        string message,
        LogLevel level)
    {
        Guid sourceId = Guid.NewGuid();

        DateTimeOffset timestamp = new(
            2026,
            8,
            4,
            21,
            30,
            0,
            TimeSpan.Zero);

        LogFingerprint fingerprint = new(
            fingerprintValue,
            message.ToLowerInvariant());

        LogGroupSample sample = new(
            sourceId,
            1,
            timestamp,
            level,
            message);

        return new LogGroupSummary(
            fingerprint,
            occurrenceCount: 1,
            firstSeen: timestamp,
            lastSeen: timestamp,
            highestLevel: level,
            representativeMessage: message,
            services: [],
            exceptionTypes: [],
            statusCodes: [],
            samples:
            [
                sample
            ]);
    }

    private sealed class FakeRule
        : IIncidentDiagnosticRule
    {
        private readonly string _targetFingerprint;

        private readonly IncidentPriority _priority;

        private readonly double _confidence;

        public string Id { get; }

        public string Name =>
            Id;

        public int Order { get; }

        public FakeRule(
            string id,
            string targetFingerprint,
            IncidentPriority priority,
            double confidence,
            int order)
        {
            Id = id;
            _targetFingerprint =
                targetFingerprint;
            _priority = priority;
            _confidence = confidence;
            Order = order;
        }

        public IncidentDiagnosis? Evaluate(
            IncidentDiagnosticContext context)
        {
            if (
                !string.Equals(
                    context.Group.Fingerprint.Value,
                    _targetFingerprint,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new IncidentDiagnosis(
                Id,
                $"Diagnosis for {Id}",
                "A test diagnosis was detected.",
                _priority,
                _confidence,
                context.Group.Fingerprint.Value,
                [
                    new DiagnosticEvidence(
                        "test-evidence",
                        "Test evidence",
                        context.Group.RepresentativeMessage)
                ],
                [
                    "Review the detected test incident."
                ],
                context.AnalyzedAt);
        }
    }
}