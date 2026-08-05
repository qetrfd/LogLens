using LogLens.Application;
using LogLens.Infrastructure;

namespace LogLens.Desktop.Services;

public sealed class DesktopLogAnalysisService
    : IDesktopLogAnalysisService
{
    private readonly LogFileDiagnosticService
        _diagnosticService;

    public DesktopLogAnalysisService()
    {
        _diagnosticService =
            DefaultLogAnalysisFactory
                .CreateFileDiagnosticService();
    }

    public Task<LogFileDiagnosticResult> AnalyzeAsync(
        string filePath,
        IProgress<LogReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            filePath);

        string fullPath =
            Path.GetFullPath(filePath.Trim());

        string sourceName =
            Path.GetFileNameWithoutExtension(
                fullPath);

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            sourceName = "Archivo";
        }

        LogReadRequest request = new(
            Guid.NewGuid(),
            fullPath,
            progressIntervalLines: 250);

        LogGroupingOptions groupingOptions = new(
            sampleLimit: 5,
            includeUnknownLevels: true);

        return _diagnosticService.DiagnoseAsync(
            request,
            sourceName,
            groupingOptions,
            progress,
            cancellationToken);
    }
}