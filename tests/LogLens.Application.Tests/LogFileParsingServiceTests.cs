using System.Runtime.CompilerServices;
using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogFileParsingServiceTests
{
    [Fact]
    public async Task ParseAsyncCountsParsedAndUnparsedLines()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine[] lines =
        [
            new(sourceId, 1, "Info message"),
            new(sourceId, 2, "Error message"),
            new(sourceId, 3, "Ignored")
        ];

        LogFileParsingService service = new(
            new FakeReader(lines),
            new FakeParser());

        LogReadRequest request = new(
            sourceId,
            "application.log");

        LogFileParsingResult result =
            await service.ParseAsync(
                request,
                "Application",
                previewLimit: 10);

        Assert.Equal(3, result.TotalLines);
        Assert.Equal(2, result.ParsedLines);
        Assert.Equal(1, result.UnparsedLines);
        Assert.Equal(2, result.Preview.Count);
        Assert.Equal(66.66666666666667,result.ParsedPercentage, precision: 10);
        Assert.Equal(1, result.LevelCounts[LogLevel.Information]);
        Assert.Equal(1, result.LevelCounts[LogLevel.Error]);
        Assert.Equal(2, result.ParserCounts["Fake parser"]);
    }

    [Fact]
    public async Task ParseAsyncAppliesPreviewLimit()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine[] lines =
        [
            new(sourceId, 1, "Info one"),
            new(sourceId, 2, "Info two"),
            new(sourceId, 3, "Info three")
        ];

        LogFileParsingService service = new(
            new FakeReader(lines),
            new FakeParser());

        LogFileParsingResult result =
            await service.ParseAsync(
                new LogReadRequest(
                    sourceId,
                    "application.log"),
                "Application",
                previewLimit: 1);

        Assert.Equal(3, result.ParsedLines);
        Assert.Single(result.Preview);
        Assert.Equal(
            "Info one",
            result.Preview[0].Message);
    }

    [Fact]
    public async Task ParseAsyncRejectsNegativePreviewLimit()
    {
        Guid sourceId = Guid.NewGuid();

        LogFileParsingService service = new(
            new FakeReader([]),
            new FakeParser());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () =>
            {
                await service.ParseAsync(
                    new LogReadRequest(
                        sourceId,
                        "application.log"),
                    "Application",
                    previewLimit: -1);
            });
    }

    private sealed class FakeReader
        : ILogFileReader
    {
        private readonly IReadOnlyList<RawLogLine> _lines;

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
        public string Name => "Fake parser";

        public int Priority => 1;

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

            LogLevel level = line.Text.Contains(
                "Error",
                StringComparison.Ordinal)
                ? LogLevel.Error
                : LogLevel.Information;

            ParsedLogLine parsed = new(
                line.SourceId,
                line.LineNumber,
                null,
                level,
                line.Text,
                line.Text);

            return LogParseResult.Parsed(
                Name,
                parsed);
        }
    }
}