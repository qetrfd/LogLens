using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogFileGroupingResultTests
{
    [Fact]
    public void ConstructorCalculatesPercentages()
    {
        Guid sourceId = Guid.NewGuid();

        LogGroupSummary recurringGroup =
            CreateGroup(
                sourceId,
                "database-failure",
                "Database failed",
                occurrenceCount: 2,
                lineNumber: 1);

        LogGroupSummary uniqueGroup =
            CreateGroup(
                sourceId,
                "request-timeout",
                "Request timeout",
                occurrenceCount: 1,
                lineNumber: 3);

        LogGroupingResult grouping = new(
            totalEntries: 3,
            groupedEntries: 3,
            excludedEntries: 0,
            groups:
            [
                recurringGroup,
                uniqueGroup
            ],
            completedAt:
                DateTimeOffset.UtcNow);

        LogFileGroupingResult result = new(
            sourceId,
            "Application",
            "application.log",
            totalLines: 4,
            parsedLines: 3,
            unparsedLines: 1,
            grouping,
            DateTimeOffset.UtcNow);

        Assert.Equal(
            75d,
            result.ParsedPercentage);

        Assert.InRange(
            result.RecurringEntryPercentage,
            66.6666666666,
            66.6666666667);

        Assert.Equal(
            2,
            result.GroupCount);

        Assert.Equal(
            1,
            result.RecurringGroupCount);
    }

    [Fact]
    public void ConstructorRejectsDifferentParsingCount()
    {
        Guid sourceId = Guid.NewGuid();

        LogGroupSummary group =
            CreateGroup(
                sourceId,
                "database-failure",
                "Database failed",
                occurrenceCount: 1,
                lineNumber: 1);

        LogGroupingResult grouping = new(
            totalEntries: 1,
            groupedEntries: 1,
            excludedEntries: 0,
            groups:
            [
                group
            ],
            completedAt:
                DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            new LogFileGroupingResult(
                sourceId,
                "Application",
                "application.log",
                totalLines: 2,
                parsedLines: 2,
                unparsedLines: 0,
                grouping,
                DateTimeOffset.UtcNow));
    }

    private static LogGroupSummary CreateGroup(
        Guid sourceId,
        string fingerprintValue,
        string message,
        long occurrenceCount,
        long lineNumber)
    {
        LogFingerprint fingerprint = new(
            fingerprintValue,
            message.ToLowerInvariant());

        LogGroupSample sample = new(
            sourceId,
            lineNumber,
            DateTimeOffset.UtcNow,
            LogLevel.Error,
            message,
            "API",
            "SocketException",
            503);

        return new LogGroupSummary(
            fingerprint,
            occurrenceCount,
            sample.Timestamp,
            sample.Timestamp,
            LogLevel.Error,
            message,
            ["API"],
            ["SocketException"],
            [503],
            [sample]);
    }
}