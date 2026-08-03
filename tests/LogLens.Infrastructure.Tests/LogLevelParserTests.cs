using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class LogLevelParserTests
{
    [Theory]
    [InlineData("TRACE", LogLevel.Trace)]
    [InlineData("debug", LogLevel.Debug)]
    [InlineData("INFO", LogLevel.Information)]
    [InlineData("[WARNING]", LogLevel.Warning)]
    [InlineData("err", LogLevel.Error)]
    [InlineData("fatal", LogLevel.Critical)]
    [InlineData("panic", LogLevel.Critical)]
    public void ParseRecognizesKnownLevels(
        string value,
        LogLevel expected)
    {
        LogLevel result = LogLevelParser.Parse(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("successful")]
    [InlineData("random")]
    public void ParseReturnsUnknownForUnsupportedValues(
        string value)
    {
        LogLevel result = LogLevelParser.Parse(value);

        Assert.Equal(LogLevel.Unknown, result);
    }

    [Fact]
    public void TryParseReturnsTrueForRecognizedLevel()
    {
        bool success = LogLevelParser.TryParse(
            "ERROR",
            out LogLevel level);

        Assert.True(success);
        Assert.Equal(LogLevel.Error, level);
    }

    [Fact]
    public void TryParseReturnsFalseForUnknownLevel()
    {
        bool success = LogLevelParser.TryParse(
            "completed",
            out LogLevel level);

        Assert.False(success);
        Assert.Equal(LogLevel.Unknown, level);
    }

    [Theory]
    [InlineData(
        "A fatal error stopped the service",
        LogLevel.Critical)]
    [InlineData(
        "Database connection failed",
        LogLevel.Error)]
    [InlineData(
        "Warning: request is slow",
        LogLevel.Warning)]
    [InlineData(
        "Debug information",
        LogLevel.Debug)]
    [InlineData(
        "Trace started",
        LogLevel.Trace)]
    [InlineData(
        "Info: application ready",
        LogLevel.Information)]
    public void InferFromTextDetectsLevel(
        string text,
        LogLevel expected)
    {
        LogLevel result =
            LogLevelParser.InferFromText(text);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void InferFromTextReturnsUnknownWithoutIndicators()
    {
        LogLevel result =
            LogLevelParser.InferFromText(
                "Application started successfully");

        Assert.Equal(LogLevel.Unknown, result);
    }
}