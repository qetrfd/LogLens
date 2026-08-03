using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class LogTimestampParserTests
{
    [Theory]
    [InlineData("2026-08-02T22:10:15Z")]
    [InlineData("2026-08-02 22:10:15")]
    [InlineData("2026/08/02 22:10:15")]
    [InlineData("02/08/2026 22:10:15")]
    [InlineData("[2026-08-02T22:10:15Z]")]
    public void TryParseRecognizesSupportedFormats(
        string value)
    {
        bool success = LogTimestampParser.TryParse(
            value,
            out DateTimeOffset timestamp);

        Assert.True(success);
        Assert.Equal(2026, timestamp.Year);
        Assert.Equal(8, timestamp.Month);
        Assert.Equal(2, timestamp.Day);
        Assert.Equal(22, timestamp.Hour);
        Assert.Equal(10, timestamp.Minute);
        Assert.Equal(15, timestamp.Second);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-date")]
    [InlineData("2026-99-99")]
    public void TryParseRejectsInvalidValues(
        string value)
    {
        bool success = LogTimestampParser.TryParse(
            value,
            out _);

        Assert.False(success);
    }

    [Fact]
    public void TryParseUnixReadsSeconds()
    {
        long unixSeconds = 1_659_472_615;

        bool success = LogTimestampParser.TryParseUnix(
            unixSeconds,
            out DateTimeOffset timestamp);

        Assert.True(success);
        Assert.Equal(
            DateTimeOffset.FromUnixTimeSeconds(unixSeconds),
            timestamp);
    }

    [Fact]
    public void TryParseUnixReadsMilliseconds()
    {
        long unixMilliseconds = 1_659_472_615_000;

        bool success = LogTimestampParser.TryParseUnix(
            unixMilliseconds,
            out DateTimeOffset timestamp);

        Assert.True(success);
        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(
                unixMilliseconds),
            timestamp);
    }

    [Fact]
    public void TryParseUnixRejectsOutOfRangeValue()
    {
        bool success = LogTimestampParser.TryParseUnix(
            long.MaxValue,
            out _);

        Assert.False(success);
    }
}