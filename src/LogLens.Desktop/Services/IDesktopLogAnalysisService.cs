using LogLens.Application;

namespace LogLens.Desktop.Services;

public interface IDesktopLogAnalysisService
{
    Task<LogFileDiagnosticResult> AnalyzeAsync(
        string filePath,
        IProgress<LogReadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}