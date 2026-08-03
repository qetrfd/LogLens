namespace LogLens.Core;

public sealed record AnalysisSession
{
    public Guid Id { get; }

    public string Name { get; }

    public IReadOnlyList<Guid> SourceIds { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? CompletedAt { get; }

    public int TotalEntries { get; }

    public int PatternCount { get; }

    public int IncidentCount { get; }

    public bool IsCompleted => CompletedAt.HasValue;

    public AnalysisSession(
        Guid id,
        string name,
        IEnumerable<Guid> sourceIds,
        DateTimeOffset startedAt,
        DateTimeOffset? completedAt,
        int totalEntries,
        int patternCount,
        int incidentCount)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la sesión no puede estar vacío.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(sourceIds);

        if (completedAt.HasValue && completedAt.Value < startedAt)
        {
            throw new ArgumentException(
                "La sesión no puede terminar antes de haber comenzado.");
        }

        if (totalEntries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalEntries));
        }

        if (patternCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(patternCount));
        }

        if (incidentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(incidentCount));
        }

        Guid[] normalizedSourceIds = sourceIds
            .Where(sourceId => sourceId != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedSourceIds.Length == 0)
        {
            throw new ArgumentException(
                "La sesión debe contener al menos una fuente.",
                nameof(sourceIds));
        }

        Id = id;
        Name = name.Trim();
        SourceIds = normalizedSourceIds;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        TotalEntries = totalEntries;
        PatternCount = patternCount;
        IncidentCount = incidentCount;
    }
}
