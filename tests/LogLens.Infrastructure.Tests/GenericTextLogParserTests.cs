using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class GenericTextLogParserTests
{
    [Fact]
    public void ParseExtractsStructuredInformation()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            18,
            "2026-08-02T22:10:15Z ERROR [API] " +
            "SocketException status=503 after 245 ms " +
            "requestId=req-123");

        LogParserContext context = new(
            sourceId,
            "API principal",
            "application.log");

        GenericTextLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.Equal("Generic text", result.ParserName);
        Assert.NotNull(result.ParsedLine);

        ParsedLogLine parsed = result.ParsedLine;

        Assert.Equal(18, parsed.LineNumber);
        Assert.Equal(LogLevel.Error, parsed.Level);
        Assert.Equal("API", parsed.Service);
        Assert.Equal("SocketException", parsed.ExceptionType);
        Assert.Equal(503, parsed.StatusCode);
        Assert.Equal(245, parsed.DurationMilliseconds);
        Assert.Equal("req-123", parsed.CorrelationId);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                2,
                22,
                10,
                15,
                TimeSpan.Zero),
            parsed.Timestamp);

        Assert.Equal(
            "Generic text",
            parsed.Metadata["parser"]);

        Assert.Equal(
            "API principal",
            parsed.Metadata["sourceName"]);
    }

    [Fact]
    public void ParseInfersLevelFromMessage()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            1,
            "Database connection failed");

        LogParserContext context = new(
            sourceId,
            "Database",
            "database.log");

        GenericTextLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.NotNull(result.ParsedLine);
        Assert.Equal(
            LogLevel.Error,
            result.ParsedLine.Level);
    }

    [Fact]
    public void ParseAcceptsPlainInformationalMessage()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            1,
            "Application started successfully");

        LogParserContext context = new(
            sourceId,
            "Application",
            "application.log");

        GenericTextLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.True(result.Success);
        Assert.NotNull(result.ParsedLine);

        Assert.Equal(
            LogLevel.Unknown,
            result.ParsedLine.Level);

        Assert.Equal(
            "Application started successfully",
            result.ParsedLine.Message);
    }

    [Fact]
    public void ParseRejectsEmptyLine()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine line = new(
            sourceId,
            1,
            "   ");

        LogParserContext context = new(
            sourceId,
            "Application",
            "application.log");

        GenericTextLogParser parser = new();

        LogParseResult result = parser.Parse(
            line,
            context);

        Assert.False(result.Success);
        Assert.Null(result.ParsedLine);
        Assert.NotNull(result.FailureReason);
    }
}