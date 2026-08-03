using LogLens.Core;
using Xunit;

namespace LogLens.Core.Tests;

public sealed class LogFileSupportTests
{
    [Theory]
    [InlineData("server.log")]
    [InlineData("application.txt")]
    [InlineData("events.jsonl")]
    [InlineData("events.ndjson")]
    [InlineData(".LOG")]
    public void SupportedExtensionsAreAccepted(string value)
    {
        Assert.True(
            SupportedLogFileExtensions.IsSupported(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("archive.zip")]
    [InlineData("report.csv")]
    [InlineData("database.sqlite")]
    public void UnsupportedExtensionsAreRejected(string value)
    {
        Assert.False(
            SupportedLogFileExtensions.IsSupported(value));
    }

    [Fact]
    public void RawLogLineReportsEmptyContent()
    {
        RawLogLine line = new(
            Guid.NewGuid(),
            1,
            "   ");

        Assert.True(line.IsEmpty);
    }

    [Fact]
    public void RawLogLineRejectsInvalidLineNumber()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RawLogLine(
                Guid.NewGuid(),
                0,
                "Mensaje"));
    }
}
