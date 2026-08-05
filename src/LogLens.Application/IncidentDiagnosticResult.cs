using LogLens.Core;

namespace LogLens.Application;

public sealed record IncidentDiagnosticResult
{
    public int TotalGroups { get; }

    public int DiagnosedGroupCount { get; }

    public IReadOnlyList<IncidentDiagnosis> Diagnoses { get; }

    public IReadOnlyDictionary<IncidentPriority, int>
        PriorityCounts { get; }

    public DateTimeOffset AnalyzedAt { get; }

    public int DiagnosisCount =>
        Diagnoses.Count;

    public int ImmediateAttentionCount =>
        Diagnoses.Count(
            diagnosis =>
                diagnosis.RequiresImmediateAttention);

    public bool HasCriticalIncidents =>
        Diagnoses.Any(
            diagnosis =>
                diagnosis.Priority ==
                IncidentPriority.Critical);

    public IncidentDiagnosticResult(
        int totalGroups,
        IEnumerable<IncidentDiagnosis> diagnoses,
        DateTimeOffset analyzedAt)
    {
        ArgumentNullException.ThrowIfNull(diagnoses);

        if (totalGroups < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalGroups),
                "La cantidad total de grupos no puede ser negativa.");
        }

        IncidentDiagnosis[] diagnosisValues =
            diagnoses
                .Where(diagnosis => diagnosis is not null)
                .ToArray();

        int diagnosedGroupCount =
            diagnosisValues
                .Select(
                    diagnosis =>
                        diagnosis.Fingerprint)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .Count();

        if (diagnosedGroupCount > totalGroups)
        {
            throw new ArgumentException(
                "La cantidad de grupos diagnosticados no puede superar el total.",
                nameof(diagnoses));
        }

        Dictionary<IncidentPriority, int>
            priorityCounts = [];

        foreach (
            IncidentDiagnosis diagnosis
            in diagnosisValues)
        {
            priorityCounts.TryGetValue(
                diagnosis.Priority,
                out int currentCount);

            priorityCounts[diagnosis.Priority] =
                currentCount + 1;
        }

        TotalGroups = totalGroups;
        DiagnosedGroupCount = diagnosedGroupCount;
        Diagnoses = diagnosisValues;
        PriorityCounts = priorityCounts;
        AnalyzedAt = analyzedAt;
    }
}