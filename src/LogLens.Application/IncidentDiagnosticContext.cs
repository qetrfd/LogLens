using LogLens.Core;

namespace LogLens.Application;

public sealed record IncidentDiagnosticContext
{
    public LogGroupSummary Group { get; }

    public long TotalEntries { get; }

    public int TotalGroups { get; }

    public DateTimeOffset AnalyzedAt { get; }

    public double GroupPercentage =>
        TotalEntries == 0
            ? 0
            : Group.OccurrenceCount * 100d / TotalEntries;

    public IncidentDiagnosticContext(
        LogGroupSummary group,
        long totalEntries,
        int totalGroups,
        DateTimeOffset analyzedAt)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (totalEntries < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalEntries),
                "El total de entradas debe ser mayor que cero.");
        }

        if (totalGroups < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalGroups),
                "El total de grupos debe ser mayor que cero.");
        }

        if (group.OccurrenceCount > totalEntries)
        {
            throw new ArgumentException(
                "Las apariciones del grupo no pueden superar el total de entradas.",
                nameof(group));
        }

        Group = group;
        TotalEntries = totalEntries;
        TotalGroups = totalGroups;
        AnalyzedAt = analyzedAt;
    }
}