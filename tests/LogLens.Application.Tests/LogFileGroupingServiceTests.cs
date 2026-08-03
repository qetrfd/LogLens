using System.Runtime.CompilerServices;
using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogFileGroupingServiceTests
{
    [Fact]
    public async Task GroupAsyncReadsParsesAndGroupsFileLines()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine[] lines =
        [
            new(
                sourceId,
                1,
                "Database failed for request 100"),

            new(
                sourceId,
                2,
                "Database failed for request 200"),

            new(
                sourceId,
                3,
                "Unknown message"),

            new(
                sourceId,
                4,
                "Ignored")
        ];

        LogFileGroupingService service = new(
            new FakeReader(lines),
            new FakeParser(),
            new FakeFingerprintGenerator());

        LogReadRequest request = new(
            sourceId,
            "application.log");

        LogFileGroupingResult result =
            await service.GroupAsync(
                request,
                "Application",
                new LogGroupingOptions(
                    sampleLimit: 2,
                    includeUnknownLevels: false));

        Assert.Equal(sourceId, result.SourceId);
        Assert.Equal("Application", result.SourceName);
        Assert.Equal(4, result.TotalLines);
        Assert.Equal(3, result.ParsedLines);
        Assert.Equal(1, result.UnparsedLines);

        Assert.Equal(
            75,
            result.ParsedPercentage);

        Assert.Equal(
            3,
            result.Grouping.TotalEntries);

        Assert.Equal(
            2,
            result.Grouping.GroupedEntries);

        Assert.Equal(
            1,
            result.Grouping.ExcludedEntries);

        Assert.Equal(1, result.GroupCount);
        Assert.Equal(1, result.RecurringGroupCount);
        Assert.Equal(100, result.RecurringEntryPercentage);

        LogGroupSummary group =
            Assert.Single(result.Groups);

        Assert.Equal(2, group.OccurrenceCount);
        Assert.Equal(2, group.Samples.Count);
        Assert.Equal(LogLevel.Error, group.HighestLevel);
    }

    [Fact]
    public async Task GroupAsyncReturnsEmptyResultForEmptyFile()
    {
        Guid sourceId = Guid.NewGuid();

        LogFileGroupingService service = new(
            new FakeReader([]),
            new FakeParser(),
            new FakeFingerprintGenerator());

        LogFileGroupingResult result =
            await service.GroupAsync(
                new LogReadRequest(
                    sourceId,
                    "empty.log"),
                "Empty");

        Assert.Equal(0, result.TotalLines);
        Assert.Equal(0, result.ParsedLines);
        Assert.Equal(0, result.UnparsedLines);
        Assert.Equal(0, result.GroupCount);
        Assert.Equal(0, result.ParsedPercentage);
        Assert.Equal(0, result.RecurringEntryPercentage);
    }

    [Fact]
    public async Task GroupAsyncPassesProgressToReader()
    {
        Guid sourceId = Guid.NewGuid();

        FakeReader reader = new(
        [
            new RawLogLine(
                sourceId,
                1,
                "Database failed")
        ]);

        LogFileGroupingService service = new(
            reader,
            new FakeParser(),
            new FakeFingerprintGenerator());

        Progress<LogReadProgress> progress = new();

        await service.GroupAsync(
            new LogReadRequest(
                sourceId,
                "application.log"),
            "Application",
            progress: progress);

        Assert.Same(
            progress,
            reader.ReceivedProgress);
    }

    private sealed class FakeReader
        : ILogFileReader
    {
        private readonly IReadOnlyList<RawLogLine> _lines;

        public IProgress<LogReadProgress>? ReceivedProgress
        {
            get;
            private set;
        }

        public FakeReader(
            IReadOnlyList<RawLogLine> lines)
        {
            _lines = lines;
        }

        public async IAsyncEnumerable<RawLogLine> ReadAsync(
            LogReadRequest request,
            IProgress<LogReadProgress>? progress = null,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
        {
            ReceivedProgress = progress;

            foreach (RawLogLine line in _lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Yield();

                yield return line;
            }
        }
    }

    private sealed class FakeParser
        : ILogLineParser
    {
        public string Name =>
            "Fake parser";

        public int Priority =>
            1;

        public LogParseResult Parse(
            RawLogLine line,
            LogParserContext context)
        {
            if (
                string.Equals(
                    line.Text,
                    "Ignored",
                    StringComparison.Ordinal))
            {
                return LogParseResult.NotMatched(
                    Name,
                    "Ignored");
            }

            LogLevel level = string.Equals(
                line.Text,
                "Unknown message",
                StringComparison.Ordinal)
                ? LogLevel.Unknown
                : LogLevel.Error;

            ParsedLogLine parsedLine = new(
                line.SourceId,
                line.LineNumber,
                null,
                level,
                line.Text,
                line.Text,
                context.SourceName);

            return LogParseResult.Parsed(
                Name,
                parsedLine);
        }
    }

    private sealed class FakeFingerprintGenerator
        : ILogFingerprintGenerator
    {
        public LogFingerprint Generate(
            ParsedLogLine line)
        {
            return new LogFingerprint(
                "database-failure",
                "database failed for request <number>");
        }
    }
}