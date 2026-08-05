using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class CriticalAndRecurringDiagnosticRuleTests
{
    [Fact]
    public void CriticalRuleCreatesCriticalDiagnosis()
    {
        LogGroupSummary group = CreateGroup(
            "critical-database",
            "Database unavailable",
            LogLevel.Critical,
            occurrenceCount: 2);

        IncidentDiagnosticContext context =
            CreateContext(
                group,
                totalEntries: 5,
                totalGroups: 2);

        CriticalIncidentDiagnosticRule rule = new();

        IncidentDiagnosis diagnosis =
            Assert.IsType<IncidentDiagnosis>(
                rule.Evaluate(context));

        Assert.Equal(
            "critical-log-level",
            diagnosis.RuleId);

        Assert.Equal(
            IncidentPriority.Critical,
            diagnosis.Priority);

        Assert.True(
            diagnosis.RequiresImmediateAttention);

        Assert.True(
            diagnosis.ConfidencePercentage >= 92);

        Assert.NotEmpty(diagnosis.Evidence);
        Assert.NotEmpty(diagnosis.RecommendedActions);
    }

    [Fact]
    public void CriticalRuleIgnoresNonCriticalGroup()
    {
        LogGroupSummary group = CreateGroup(
            "error-database",
            "Database unavailable",
            LogLevel.Error,
            occurrenceCount: 2);

        CriticalIncidentDiagnosticRule rule = new();

        IncidentDiagnosis? diagnosis =
            rule.Evaluate(
                CreateContext(
                    group,
                    totalEntries: 2,
                    totalGroups: 1));

        Assert.Null(diagnosis);
    }

    [Fact]
    public void RecurringRuleCreatesHighPriorityDiagnosis()
    {
        LogGroupSummary group = CreateGroup(
            "recurring-database",
            "Database connection failed",
            LogLevel.Error,
            occurrenceCount: 6);

        RecurringFailureDiagnosticRule rule = new();

        IncidentDiagnosis diagnosis =
            Assert.IsType<IncidentDiagnosis>(
                rule.Evaluate(
                    CreateContext(
                        group,
                        totalEntries: 10,
                        totalGroups: 2)));

        Assert.Equal(
            "recurring-failure",
            diagnosis.RuleId);

        Assert.Equal(
            IncidentPriority.High,
            diagnosis.Priority);

        Assert.True(
            diagnosis.RequiresImmediateAttention);

        Assert.Contains(
            diagnosis.Evidence,
            evidence =>
                evidence.Code ==
                "occurrence-count");
    }

    [Fact]
    public void RecurringRuleIgnoresSingleFailure()
    {
        LogGroupSummary group = CreateGroup(
            "single-database",
            "Database connection failed",
            LogLevel.Error,
            occurrenceCount: 1);

        RecurringFailureDiagnosticRule rule = new();

        IncidentDiagnosis? diagnosis =
            rule.Evaluate(
                CreateContext(
                    group,
                    totalEntries: 1,
                    totalGroups: 1));

        Assert.Null(diagnosis);
    }

    private static IncidentDiagnosticContext
        CreateContext(
            LogGroupSummary group,
            long totalEntries,
            int totalGroups)
    {
        return new IncidentDiagnosticContext(
            group,
            totalEntries,
            totalGroups,
            new DateTimeOffset(
                2026,
                8,
                4,
                21,
                45,
                0,
                TimeSpan.Zero));
    }

    private static LogGroupSummary CreateGroup(
        string fingerprintValue,
        string message,
        LogLevel level,
        long occurrenceCount)
    {
        Guid sourceId = Guid.NewGuid();

        DateTimeOffset firstSeen = new(
            2026,
            8,
            4,
            21,
            30,
            0,
            TimeSpan.Zero);

        DateTimeOffset lastSeen =
            firstSeen.AddMinutes(5);

        LogGroupSample sample = new(
            sourceId,
            1,
            firstSeen,
            level,
            message,
            "API",
            "SocketException",
            503);

        return new LogGroupSummary(
            new LogFingerprint(
                fingerprintValue,
                message.ToLowerInvariant()),
            occurrenceCount,
            firstSeen,
            lastSeen,
            level,
            message,
            ["API"],
            ["SocketException"],
            [503],
            [sample]);
    }
}