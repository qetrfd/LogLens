using LogLens.Core;

namespace LogLens.Application;

public sealed record LogFileDiagnosticResult
{
    public LogFileGroupingResult FileGrouping { get; }

    public IncidentDiagnosticResult Diagnostics { get; }

    public DateTimeOffset CompletedAt { get; }

    public Guid SourceId =>
        FileGrouping.SourceId;

    public string SourceName =>
        FileGrouping.SourceName;

    public string FilePath =>
        FileGrouping.FilePath;

    public long TotalLines =>
        FileGrouping.TotalLines;

    public long ParsedLines =>
        FileGrouping.ParsedLines;

    public long UnparsedLines =>
        FileGrouping.UnparsedLines;

    public IReadOnlyList<LogGroupSummary> Groups =>
        FileGrouping.Groups;

    public IReadOnlyList<IncidentDiagnosis> Diagnoses =>
        Diagnostics.Diagnoses;

    public int GroupCount =>
        FileGrouping.GroupCount;

    public int DiagnosisCount =>
        Diagnostics.DiagnosisCount;

    public int ImmediateAttentionCount =>
        Diagnostics.ImmediateAttentionCount;

    public bool HasCriticalIncidents =>
        Diagnostics.HasCriticalIncidents;

    public LogFileDiagnosticResult(
        LogFileGroupingResult fileGrouping,
        IncidentDiagnosticResult diagnostics,
        DateTimeOffset completedAt)
    {
        ArgumentNullException.ThrowIfNull(fileGrouping);
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (
            diagnostics.TotalGroups !=
            fileGrouping.GroupCount)
        {
            throw new ArgumentException(
                "La cantidad de grupos del diagnóstico debe coincidir con el agrupamiento.",
                nameof(diagnostics));
        }

        FileGrouping = fileGrouping;
        Diagnostics = diagnostics;
        CompletedAt = completedAt;
    }
}