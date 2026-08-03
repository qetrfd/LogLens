using System.Runtime.CompilerServices;
using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogParsingServiceTests
{
    [Fact]
    public void ParseReturnsParserResult()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            12,
            "Application ready");

        LogParserContext context = new(
            sourceId,
            "Application",
            "application.log");

        LogParsingService service = new(
            new SuccessfulParser());

        LogParseResult result =
            service.Parse(
                line,
                context);

        Assert.True(result.Success);
        Assert.Equal(
            "Successful parser",
            result.ParserName);

        Assert.NotNull(result.ParsedLine);

        Assert.Equal(
            "Application ready",
            result.ParsedLine.Message);

        Assert.Equal(
            LogLevel.Information,
            result.ParsedLine.Level);
    }

    [Fact]
    public void ParseRejectsDifferentSourceIdentifiers()
    {
        RawLogLine line = new(
            Guid.NewGuid(),
            1,
            "Application ready");

        LogParserContext context = new(
            Guid.NewGuid(),
            "Application",
            "application.log");

        LogParsingService service = new(
            new SuccessfulParser());

        Assert.Throws<ArgumentException>(() =>
            service.Parse(
                line,
                context));
    }

    [Fact]
    public async Task ParseAsyncProcessesAllLines()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine[] lines =
        [
            new(
                sourceId,
                1,
                "Application ready"),

            new(
                sourceId,
                2,
                "Request completed"),

            new(
                sourceId,
                3,
                "Database connected")
        ];

        LogParserContext context = new(
            sourceId,
            "Application",
            "application.log");

        LogParsingService service = new(
            new SuccessfulParser());

        List<LogParseResult> results = [];

        await foreach (
            LogParseResult result in service.ParseAsync(
                ToAsyncEnumerable(lines),
                context))
        {
            results.Add(result);
        }

        Assert.Equal(
            3,
            results.Count);

        Assert.All(
            results,
            result => Assert.True(result.Success));

        Assert.All(
            results,
            result => Assert.Equal(
                "Successful parser",
                result.ParserName));
    }

    [Fact]
    public async Task ParseAsyncSupportsCancellation()
    {
        Guid sourceId = Guid.NewGuid();

        LogParserContext context = new(
            sourceId,
            "Application",
            "application.log");

        using CancellationTokenSource cancellationSource =
            new();

        cancellationSource.Cancel();

        LogParsingService service = new(
            new SuccessfulParser());

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
            {
                await foreach (
                    LogParseResult _ in service.ParseAsync(
                        CreateDelayedLines(sourceId),
                        context,
                        cancellationSource.Token))
                {
                }
            });
    }

    private static async IAsyncEnumerable<RawLogLine>
        ToAsyncEnumerable(
            IEnumerable<RawLogLine> lines,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        foreach (RawLogLine line in lines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Yield();

            yield return line;
        }
    }

    private static async IAsyncEnumerable<RawLogLine>
        CreateDelayedLines(
            Guid sourceId,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Yield();

        yield return new RawLogLine(
            sourceId,
            1,
            "Application ready");
    }

    private sealed class SuccessfulParser
        : ILogLineParser
    {
        public string Name =>
            "Successful parser";

        public int Priority =>
            1;

        public LogParseResult Parse(
            RawLogLine line,
            LogParserContext context)
        {
            ParsedLogLine parsedLine = new(
                line.SourceId,
                line.LineNumber,
                null,
                LogLevel.Information,
                line.Text,
                line.Text,
                context.SourceName);

            return LogParseResult.Parsed(
                Name,
                parsedLine);
        }
    }
}