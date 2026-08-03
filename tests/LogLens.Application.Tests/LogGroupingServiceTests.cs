using System.Runtime.CompilerServices;
using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogGroupingServiceTests
{
    [Fact]
    public async Task GroupAsyncCombinesEquivalentEntries()
    {
        Guid sourceId = Guid.NewGuid();

        ParsedLogLine[] lines =
        [
            CreateLine(
                sourceId,
                1,
                "Database connection failed",
                LogLevel.Error,
                new DateTimeOffset(
                    2026,
                    8,
                    2,
                    22,
                    10,
                    1,
                    TimeSpan.Zero),
                "API"),

            CreateLine(
                sourceId,
                2,
                "Database connection failed",
                LogLevel.Critical,
                new DateTimeOffset(
                    2026,
                    8,
                    2,
                    22,
                    10,
                    5,
                    TimeSpan.Zero),
                "Worker"),

            CreateLine(
                sourceId,
                3,
                "Database connection failed",
                LogLevel.Error,
                new DateTimeOffset(
                    2026,
                    8,
                    2,
                    22,
                    10,
                    3,
                    TimeSpan.Zero),
                "API")
        ];

        LogGroupingService service = new(
            new ConstantFingerprintGenerator());

        LogGroupingResult result =
            await service.GroupAsync(
                ToAsyncEnumerable(lines),
                new LogGroupingOptions(
                    sampleLimit: 2));

        Assert.Equal(3, result.TotalEntries);
        Assert.Equal(3, result.GroupedEntries);
        Assert.Equal(0, result.ExcludedEntries);
        Assert.Equal(1, result.GroupCount);
        Assert.Equal(1, result.RecurringGroupCount);
        Assert.Equal(3, result.AverageOccurrencesPerGroup);

        LogGroupSummary group =
            Assert.Single(result.Groups);

        Assert.Equal(3, group.OccurrenceCount);
        Assert.Equal(LogLevel.Critical, group.HighestLevel);
        Assert.True(group.IsRecurring);
        Assert.Equal(2, group.Samples.Count);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                2,
                22,
                10,
                1,
                TimeSpan.Zero),
            group.FirstSeen);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                2,
                22,
                10,
                5,
                TimeSpan.Zero),
            group.LastSeen);

        Assert.Equal(
            ["API", "Worker"],
            group.Services);
    }

    [Fact]
    public async Task GroupAsyncExcludesUnknownLevels()
    {
        Guid sourceId = Guid.NewGuid();

        ParsedLogLine[] lines =
        [
            CreateLine(
                sourceId,
                1,
                "Application ready",
                LogLevel.Unknown,
                null,
                "API"),

            CreateLine(
                sourceId,
                2,
                "Database failed",
                LogLevel.Error,
                null,
                "API")
        ];

        LogGroupingService service = new(
            new MessageFingerprintGenerator());

        LogGroupingResult result =
            await service.GroupAsync(
                ToAsyncEnumerable(lines),
                new LogGroupingOptions(
                    includeUnknownLevels: false));

        Assert.Equal(2, result.TotalEntries);
        Assert.Equal(1, result.GroupedEntries);
        Assert.Equal(1, result.ExcludedEntries);
        Assert.Single(result.Groups);

        Assert.Equal(
            "Database failed",
            result.Groups[0].RepresentativeMessage);
    }

    [Fact]
    public async Task GroupAsyncOrdersGroupsByOccurrences()
    {
        Guid sourceId = Guid.NewGuid();

        ParsedLogLine[] lines =
        [
            CreateLine(
                sourceId,
                1,
                "Database failed",
                LogLevel.Error),

            CreateLine(
                sourceId,
                2,
                "Request timeout",
                LogLevel.Warning),

            CreateLine(
                sourceId,
                3,
                "Database failed",
                LogLevel.Error)
        ];

        LogGroupingService service = new(
            new MessageFingerprintGenerator());

        LogGroupingResult result =
            await service.GroupAsync(
                ToAsyncEnumerable(lines));

        Assert.Equal(2, result.GroupCount);

        Assert.Equal(
            "Database failed",
            result.Groups[0].RepresentativeMessage);

        Assert.Equal(
            2,
            result.Groups[0].OccurrenceCount);

        Assert.Equal(
            "Request timeout",
            result.Groups[1].RepresentativeMessage);

        Assert.Equal(
            1,
            result.Groups[1].OccurrenceCount);
    }

    [Fact]
    public async Task GroupAsyncSupportsZeroSampleLimit()
    {
        Guid sourceId = Guid.NewGuid();

        ParsedLogLine[] lines =
        [
            CreateLine(
                sourceId,
                1,
                "Database failed",
                LogLevel.Error)
        ];

        LogGroupingService service = new(
            new ConstantFingerprintGenerator());

        LogGroupingResult result =
            await service.GroupAsync(
                ToAsyncEnumerable(lines),
                new LogGroupingOptions(
                    sampleLimit: 0));

        LogGroupSummary group =
            Assert.Single(result.Groups);

        Assert.Empty(group.Samples);
    }

    [Fact]
    public async Task GroupAsyncSupportsCancellation()
    {
        using CancellationTokenSource cancellationSource =
            new();

        cancellationSource.Cancel();

        LogGroupingService service = new(
            new ConstantFingerprintGenerator());

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
            {
                await service.GroupAsync(
                    CreateDelayedLines(),
                    cancellationToken:
                        cancellationSource.Token);
            });
    }

    private static ParsedLogLine CreateLine(
        Guid sourceId,
        long lineNumber,
        string message,
        LogLevel level,
        DateTimeOffset? timestamp = null,
        string? service = null)
    {
        return new ParsedLogLine(
            sourceId,
            lineNumber,
            timestamp,
            level,
            message,
            message,
            service);
    }

    private static async IAsyncEnumerable<ParsedLogLine>
        ToAsyncEnumerable(
            IEnumerable<ParsedLogLine> lines,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        foreach (ParsedLogLine line in lines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Yield();

            yield return line;
        }
    }

    private static async IAsyncEnumerable<ParsedLogLine>
        CreateDelayedLines(
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Yield();

        yield return new ParsedLogLine(
            Guid.NewGuid(),
            1,
            null,
            LogLevel.Error,
            "Database failed",
            "Database failed");
    }

    private sealed class ConstantFingerprintGenerator
        : ILogFingerprintGenerator
    {
        public LogFingerprint Generate(
            ParsedLogLine line)
        {
            return new LogFingerprint(
                "constant-fingerprint",
                "normalized message");
        }
    }

    private sealed class MessageFingerprintGenerator
        : ILogFingerprintGenerator
    {
        public LogFingerprint Generate(
            ParsedLogLine line)
        {
            string normalized =
                line.Message.Trim().ToLowerInvariant();

            return new LogFingerprint(
                normalized,
                normalized);
        }
    }
}