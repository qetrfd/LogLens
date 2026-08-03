using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class DefaultLogAnalysisFactoryTests
{
    [Fact]
    public async Task CreateFileGroupingServiceGroupsEquivalentLines()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"loglens-{Guid.NewGuid():N}.log");

        string[] lines =
        [
            "2026-08-02T22:10:15Z ERROR [API] " +
            "SocketException connection refused at " +
            "10.0.0.8:5432 after 245 ms " +
            "requestId=req-100",

            "2026-08-02T22:10:16Z ERROR [API] " +
            "SocketException connection refused at " +
            "10.0.0.9:5432 after 980 ms " +
            "requestId=req-200",

            "2026-08-02T22:10:17Z INFO [API] " +
            "Application started"
        ];

        await File.WriteAllLinesAsync(
            filePath,
            lines);

        try
        {
            LogFileGroupingService service =
                DefaultLogAnalysisFactory
                    .CreateFileGroupingService();

            LogReadRequest request = new(
                Guid.NewGuid(),
                filePath,
                progressIntervalLines: 1);

            LogFileGroupingResult result =
                await service.GroupAsync(
                    request,
                    "Integration",
                    new LogGroupingOptions(
                        sampleLimit: 2));

            Assert.Equal(
                3,
                result.TotalLines);

            Assert.Equal(
                3,
                result.ParsedLines);

            Assert.Equal(
                0,
                result.UnparsedLines);

            Assert.Equal(
                2,
                result.GroupCount);

            Assert.Equal(
                1,
                result.RecurringGroupCount);

            Assert.Equal(
                100d,
                result.ParsedPercentage);

            LogGroupSummary recurringGroup =
    Assert.Single(
        result.Groups,
        group => group.IsRecurring);

            Assert.Equal(
                2,
                recurringGroup.OccurrenceCount);

            Assert.Equal(
                LogLevel.Error,
                recurringGroup.HighestLevel);

            Assert.Equal(
                "API",
                Assert.Single(
                    recurringGroup.Services));

            Assert.Contains(
                "<ip>",
                recurringGroup
                    .Fingerprint
                    .NormalizedMessage);

            Assert.Contains(
                "<number>",
                recurringGroup
                    .Fingerprint
                    .NormalizedMessage);

            Assert.Contains(
                "<id>",
                recurringGroup
                    .Fingerprint
                    .NormalizedMessage);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void CreateFileGroupingServiceReturnsService()
    {
        LogFileGroupingService service =
            DefaultLogAnalysisFactory
                .CreateFileGroupingService();

        Assert.NotNull(service);
    }
}