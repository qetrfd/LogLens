namespace LogLens.Application;

public sealed class LogFileDiagnosticService
{
    private readonly LogFileGroupingService
        _groupingService;

    private readonly IncidentDiagnosticService
        _diagnosticService;

    public LogFileDiagnosticService(
        LogFileGroupingService groupingService,
        IncidentDiagnosticService diagnosticService)
    {
        ArgumentNullException.ThrowIfNull(
            groupingService);

        ArgumentNullException.ThrowIfNull(
            diagnosticService);

        _groupingService = groupingService;
        _diagnosticService = diagnosticService;
    }

    public async Task<LogFileDiagnosticResult>
        DiagnoseAsync(
            LogReadRequest request,
            string sourceName,
            LogGroupingOptions? groupingOptions = null,
            IProgress<LogReadProgress>? progress = null,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            sourceName);

        LogFileGroupingResult fileGrouping =
            await _groupingService.GroupAsync(
                request,
                sourceName,
                groupingOptions,
                progress,
                cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset analyzedAt =
            DateTimeOffset.UtcNow;

        IncidentDiagnosticResult diagnostics =
            _diagnosticService.Diagnose(
                fileGrouping.Grouping,
                analyzedAt);

        return new LogFileDiagnosticResult(
            fileGrouping,
            diagnostics,
            DateTimeOffset.UtcNow);
    }
}