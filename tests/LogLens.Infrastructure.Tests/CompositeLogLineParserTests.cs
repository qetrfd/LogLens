using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class CompositeLogLineParserTests
{
    [Fact]
    public void ParsePrioritizesJsonParser()
    {
        Guid sourceId = Guid.NewGuid();

        CompositeLogLineParser parser = new(
        [
            new GenericTextLogParser(),
            new JsonLineLogParser()
        ]);

        RawLogLine line = new(
            sourceId,
            1,
            """
            {"timestamp":"2026-08-02T22:10:15Z","level":"error","message":"Database failed"}
            """);

        LogParserContext context = new(
            sourceId,
            "API",
            "events.jsonl");

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.Equal("JSON lines", result.ParserName);
        Assert.NotNull(result.ParsedLine);

        Assert.Equal(
            "Database failed",
            result.ParsedLine.Message);
    }

    [Fact]
    public void ParseFallsBackToGenericText()
    {
        Guid sourceId = Guid.NewGuid();

        CompositeLogLineParser parser = new(
        [
            new GenericTextLogParser(),
            new JsonLineLogParser()
        ]);

        RawLogLine line = new(
            sourceId,
            2,
            "2026-08-02T22:10:15Z WARNING Request is slow");

        LogParserContext context = new(
            sourceId,
            "Gateway",
            "gateway.log");

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.Equal("Generic text", result.ParserName);
        Assert.NotNull(result.ParsedLine);

        Assert.Equal(
            LogLevel.Warning,
            result.ParsedLine.Level);
    }

    [Fact]
    public void ParseReturnsFailureWhenNoParserMatches()
    {
        Guid sourceId = Guid.NewGuid();

        CompositeLogLineParser parser = new(
        [
            new GenericTextLogParser(),
            new JsonLineLogParser()
        ]);

        RawLogLine line = new(
            sourceId,
            3,
            "   ");

        LogParserContext context = new(
            sourceId,
            "Application",
            "application.log");

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.False(result.Success);
        Assert.Equal("Composite", result.ParserName);
        Assert.Null(result.ParsedLine);
        Assert.NotNull(result.FailureReason);
        Assert.Contains(
            "línea está vacía",
            result.FailureReason);
    }

    [Fact]
    public void ConstructorRejectsEmptyParserCollection()
    {
        Assert.Throws<ArgumentException>(() =>
            new CompositeLogLineParser([]));
    }

    [Fact]
    public void ParsingServiceRejectsDifferentSource()
    {
        CompositeLogLineParser parser = new(
        [
            new GenericTextLogParser()
        ]);

        LogParsingService service = new(parser);

        RawLogLine line = new(
            Guid.NewGuid(),
            1,
            "Application ready");

        LogParserContext context = new(
            Guid.NewGuid(),
            "Application",
            "application.log");

        Assert.Throws<ArgumentException>(() =>
            service.Parse(
                line,
                context));
    }
}