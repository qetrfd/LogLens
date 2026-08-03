using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class DefaultLogFingerprintGeneratorTests
{
    [Fact]
    public void GenerateProducesSameFingerprintForVariableValues()
    {
        DefaultLogFingerprintGenerator generator =
            new();

        ParsedLogLine first = CreateLine(
            "Connection refused at 10.0.0.8:5432 " +
            "after 245 ms requestId=req-100");

        ParsedLogLine second = CreateLine(
            "Connection refused at 10.0.0.9:5432 " +
            "after 980 ms requestId=req-999");

        LogFingerprint firstFingerprint =
            generator.Generate(first);

        LogFingerprint secondFingerprint =
            generator.Generate(second);

        Assert.Equal(
            firstFingerprint.Value,
            secondFingerprint.Value);

        Assert.Equal(
            firstFingerprint.NormalizedMessage,
            secondFingerprint.NormalizedMessage);
    }

    [Fact]
    public void GenerateNormalizesDynamicMessageValues()
    {
        DefaultLogFingerprintGenerator generator =
            new();

        ParsedLogLine line = CreateLine(
            "2026-08-02T22:10:15Z connection failed " +
            "at 10.0.0.8:5432 after 245 ms " +
            "requestId=req-100");

        LogFingerprint fingerprint =
            generator.Generate(line);

        Assert.Contains(
            "<timestamp>",
            fingerprint.NormalizedMessage);

        Assert.Contains(
            "<ip>",
            fingerprint.NormalizedMessage);

        Assert.Contains(
            "<number>",
            fingerprint.NormalizedMessage);

        Assert.Contains(
            "<id>",
            fingerprint.NormalizedMessage);
    }

    [Fact]
    public void GenerateUsesLogLevelInFingerprint()
    {
        DefaultLogFingerprintGenerator generator =
            new();

        ParsedLogLine warning = CreateLine(
            "Request failed after 245 ms",
            LogLevel.Warning);

        ParsedLogLine error = CreateLine(
            "Request failed after 980 ms",
            LogLevel.Error);

        LogFingerprint warningFingerprint =
            generator.Generate(warning);

        LogFingerprint errorFingerprint =
            generator.Generate(error);

        Assert.NotEqual(
            warningFingerprint.Value,
            errorFingerprint.Value);
    }

    [Fact]
    public void GenerateUsesServiceInFingerprint()
    {
        DefaultLogFingerprintGenerator generator =
            new();

        ParsedLogLine apiLine = CreateLine(
            "Connection failed",
            service: "API");

        ParsedLogLine databaseLine = CreateLine(
            "Connection failed",
            service: "Database");

        LogFingerprint apiFingerprint =
            generator.Generate(apiLine);

        LogFingerprint databaseFingerprint =
            generator.Generate(databaseLine);

        Assert.NotEqual(
            apiFingerprint.Value,
            databaseFingerprint.Value);
    }

    [Fact]
    public void GenerateUsesStatusCodeInFingerprint()
    {
        DefaultLogFingerprintGenerator generator =
            new();

        ParsedLogLine notFound = CreateLine(
            "Request returned status 404",
            statusCode: 404);

        ParsedLogLine serverError = CreateLine(
            "Request returned status 500",
            statusCode: 500);

        LogFingerprint notFoundFingerprint =
            generator.Generate(notFound);

        LogFingerprint serverErrorFingerprint =
            generator.Generate(serverError);

        Assert.NotEqual(
            notFoundFingerprint.Value,
            serverErrorFingerprint.Value);
    }

    [Fact]
    public void GenerateRejectsNullLine()
    {
        DefaultLogFingerprintGenerator generator =
            new();

        Assert.Throws<ArgumentNullException>(() =>
            generator.Generate(null!));
    }

    private static ParsedLogLine CreateLine(
        string message,
        LogLevel level = LogLevel.Error,
        string? service = "API",
        int? statusCode = 503)
    {
        return new ParsedLogLine(
            Guid.NewGuid(),
            1,
            DateTimeOffset.UtcNow,
            level,
            message,
            message,
            service,
            "SocketException",
            statusCode);
    }
}