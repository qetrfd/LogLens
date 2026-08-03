using LogLens.Core;
using Xunit;

namespace LogLens.Core.Tests;

public sealed class LogGroupingDomainTests
{
    [Fact]
    public void FingerprintNormalizesValue()
    {
        LogFingerprint fingerprint = new(
            "  ABCDEF123456  ",
            "  connection failed  ");

        Assert.Equal(
            "abcdef123456",
            fingerprint.Value);

        Assert.Equal(
            "connection failed",
            fingerprint.NormalizedMessage);

        Assert.Equal(
            "abcdef123456",
            fingerprint.ToString());
    }

    [Fact]
    public void GroupSampleCanBeCreatedFromParsedLine()
    {
        Guid sourceId = Guid.NewGuid();

        DateTimeOffset timestamp = new(
            2026,
            8,
            2,
            22,
            10,
            15,
            TimeSpan.Zero);

        ParsedLogLine line = new(
            sourceId,
            8,
            timestamp,
            LogLevel.Error,
            "Database connection failed",
            "Database connection failed",
            "API",
            "SocketException",
            503);

        LogGroupSample sample =
            LogGroupSample.From(line);

        Assert.Equal(sourceId, sample.SourceId);
        Assert.Equal(8, sample.LineNumber);
        Assert.Equal(timestamp, sample.Timestamp);
        Assert.Equal(LogLevel.Error, sample.Level);

        Assert.Equal(
            "Database connection failed",
            sample.Message);

        Assert.Equal("API", sample.Service);
        Assert.Equal("SocketException", sample.ExceptionType);
        Assert.Equal(503, sample.StatusCode);
    }

    [Fact]
    public void GroupSummaryNormalizesCollections()
    {
        Guid sourceId = Guid.NewGuid();

        LogFingerprint fingerprint = new(
            "database-failure",
            "database connection failed");

        LogGroupSample sample = new(
            sourceId,
            1,
            DateTimeOffset.UtcNow,
            LogLevel.Error,
            "Database connection failed",
            "API",
            "SocketException",
            503);

        LogGroupSummary summary = new(
            fingerprint,
            occurrenceCount: 3,
            firstSeen: sample.Timestamp,
            lastSeen: sample.Timestamp,
            highestLevel: LogLevel.Error,
            representativeMessage:
                "Database connection failed",
            services:
            [
                "API",
                "api",
                "Worker"
            ],
            exceptionTypes:
            [
                "SocketException",
                "SocketException"
            ],
            statusCodes:
            [
                503,
                500,
                503
            ],
            samples:
            [
                sample
            ]);

        Assert.True(summary.IsRecurring);
        Assert.Equal(3, summary.OccurrenceCount);

        Assert.Equal(
            2,
            summary.Services.Count);

        Assert.Contains(
            "API",
            summary.Services,
            StringComparer.OrdinalIgnoreCase);

        Assert.Contains(
            "Worker",
            summary.Services,
            StringComparer.OrdinalIgnoreCase);

        Assert.Single(
            summary.ExceptionTypes);

        Assert.Equal(
            [500, 503],
            summary.StatusCodes);

        Assert.Single(
            summary.Samples);
    }

    [Fact]
    public void GroupSummaryRejectsInvalidTimestampOrder()
    {
        DateTimeOffset firstSeen =
            DateTimeOffset.UtcNow;

        DateTimeOffset lastSeen =
            firstSeen.AddMinutes(-1);

        LogFingerprint fingerprint = new(
            "database-failure",
            "database connection failed");

        Assert.Throws<ArgumentException>(() =>
            new LogGroupSummary(
                fingerprint,
                occurrenceCount: 1,
                firstSeen,
                lastSeen,
                LogLevel.Error,
                "Database connection failed",
                [],
                [],
                [],
                []));
    }

    [Fact]
    public void GroupSummaryRejectsTooManySamples()
    {
        Guid sourceId = Guid.NewGuid();

        LogGroupSample firstSample = new(
            sourceId,
            1,
            null,
            LogLevel.Error,
            "Database failed");

        LogGroupSample secondSample = new(
            sourceId,
            2,
            null,
            LogLevel.Error,
            "Database failed");

        LogFingerprint fingerprint = new(
            "database-failure",
            "database failed");

        Assert.Throws<ArgumentException>(() =>
            new LogGroupSummary(
                fingerprint,
                occurrenceCount: 1,
                firstSeen: null,
                lastSeen: null,
                highestLevel: LogLevel.Error,
                representativeMessage:
                    "Database failed",
                services: [],
                exceptionTypes: [],
                statusCodes: [],
                samples:
                [
                    firstSample,
                    secondSample
                ]));
    }
}