using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class JsonLineLogParserTests
{
    [Fact]
    public void ParseExtractsStructuredJsonFields()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            25,
            """
            {"timestamp":"2026-08-02T22:10:15Z","level":"error","message":"Database connection failed","service":"api","exceptionType":"SocketException","statusCode":503,"durationMs":245,"requestId":"req-456","environment":"production"}
            """);

        LogParserContext context = new(
            sourceId,
            "API JSON",
            "events.jsonl");

        JsonLineLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.Equal("JSON lines", result.ParserName);
        Assert.NotNull(result.ParsedLine);

        ParsedLogLine parsed = result.ParsedLine;

        Assert.Equal(25, parsed.LineNumber);
        Assert.Equal(LogLevel.Error, parsed.Level);
        Assert.Equal(
            "Database connection failed",
            parsed.Message);

        Assert.Equal("api", parsed.Service);
        Assert.Equal("SocketException", parsed.ExceptionType);
        Assert.Equal(503, parsed.StatusCode);
        Assert.Equal(245, parsed.DurationMilliseconds);
        Assert.Equal("req-456", parsed.CorrelationId);

        Assert.Equal(
            "production",
            parsed.Metadata["environment"]);

        Assert.Equal(
            "JSON lines",
            parsed.Metadata["parser"]);
    }

    [Fact]
    public void ParseReadsUnixTimestamp()
    {
        Guid sourceId = Guid.NewGuid();
        long unixSeconds = 1_659_472_615;

        RawLogLine line = new(
            sourceId,
            1,
            $$"""
            {"timestamp":{{unixSeconds}},"level":"info","message":"Application ready"}
            """);

        LogParserContext context = new(
            sourceId,
            "Application",
            "events.jsonl");

        JsonLineLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.NotNull(result.ParsedLine);

        Assert.Equal(
            DateTimeOffset.FromUnixTimeSeconds(
                unixSeconds),
            result.ParsedLine.Timestamp);
    }

    [Fact]
    public void ParseUsesAlternativePropertyNames()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            2,
            """
            {"time":"2026-08-02T22:10:15Z","severity":"warning","msg":"Request is slow","application":"gateway","latencyMs":"980","traceId":"trace-789"}
            """);

        LogParserContext context = new(
            sourceId,
            "Gateway",
            "gateway.ndjson");

        JsonLineLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.NotNull(result.ParsedLine);

        Assert.Equal(
            LogLevel.Warning,
            result.ParsedLine.Level);

        Assert.Equal(
            "Request is slow",
            result.ParsedLine.Message);

        Assert.Equal(
            "gateway",
            result.ParsedLine.Service);

        Assert.Equal(
            980,
            result.ParsedLine.DurationMilliseconds);

        Assert.Equal(
            "trace-789",
            result.ParsedLine.CorrelationId);
    }

    [Fact]
    public void ParseRejectsInvalidJson()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            1,
            """{"message":"Incomplete JSON" """);

        LogParserContext context = new(
            sourceId,
            "Application",
            "events.jsonl");

        JsonLineLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.False(result.Success);
        Assert.Null(result.ParsedLine);
    }

    [Fact]
    public void ParseRejectsJsonWithoutMessage()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            1,
            """{"level":"error","statusCode":500}""");

        LogParserContext context = new(
            sourceId,
            "Application",
            "events.jsonl");

        JsonLineLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.False(result.Success);
        Assert.Null(result.ParsedLine);
    }
}