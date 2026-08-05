using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class DefaultLogDiagnosticFactoryTests
{
    [Fact]
    public async Task DiagnosticFactoryAnalyzesCompleteLogFile()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"loglens-diagnostic-{Guid.NewGuid():N}.log");

        string[] lines =
        [
            "2026-08-04T21:00:01Z ERROR [API] " +
            "SocketException connection refused at " +
            "10.0.0.8:5432 status=503 after 245 ms " +
            "requestId=req-100",

            "2026-08-04T21:00:02Z ERROR [API] " +
            "SocketException connection refused at " +
            "10.0.0.9:5432 status=503 after 980 ms " +
            "requestId=req-200",

            "2026-08-04T21:00:03Z INFO [API] " +
            "Application started"
        ];

        await File.WriteAllLinesAsync(
            filePath,
            lines);

        try
        {
            LogFileDiagnosticService service =
                DefaultLogAnalysisFactory
                    .CreateFileDiagnosticService();

            LogFileDiagnosticResult result =
                await service.DiagnoseAsync(
                    new LogReadRequest(
                        Guid.NewGuid(),
                        filePath,
                        progressIntervalLines: 1),
                    "Integration",
                    new LogGroupingOptions(
                        sampleLimit: 2));

            Assert.Equal(3, result.TotalLines);
            Assert.Equal(3, result.ParsedLines);
            Assert.Equal(0, result.UnparsedLines);
            Assert.Equal(2, result.GroupCount);
            Assert.True(result.DiagnosisCount >= 3);
            Assert.True(result.ImmediateAttentionCount >= 1);

            Assert.Contains(
                result.Diagnoses,
                diagnosis =>
                    diagnosis.RuleId ==
                    "recurring-failure");

            Assert.Contains(
                result.Diagnoses,
                diagnosis =>
                    diagnosis.RuleId ==
                    "http-failure");

            Assert.Contains(
                result.Diagnoses,
                diagnosis =>
                    diagnosis.RuleId ==
                    "connection-failure");
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
    public void DiagnosticFactoryReturnsService()
    {
        LogFileDiagnosticService service =
            DefaultLogAnalysisFactory
                .CreateFileDiagnosticService();

        Assert.NotNull(service);
    }
}