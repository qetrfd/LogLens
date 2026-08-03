namespace LogLens.Core;

public sealed record Incident
{
    public Guid Id { get; }

    public string Title { get; }

    public string Description { get; }

    public IncidentSeverity Severity { get; }

    public IncidentStatus Status { get; }

    public string? ProbableCause { get; }

    public string? RecommendedAction { get; }

    public double Confidence { get; }

    public IReadOnlyList<Guid> PatternIds { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }

    public Incident(
        Guid id,
        string title,
        string description,
        IncidentSeverity severity,
        IncidentStatus status,
        double confidence,
        IEnumerable<Guid> patternIds,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        string? probableCause = null,
        string? recommendedAction = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del incidente no puede estar vacío.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(patternIds);

        if (confidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence),
                "La confianza debe estar entre cero y uno.");
        }

        if (updatedAt < createdAt)
        {
            throw new ArgumentException(
                "La fecha de actualización no puede ser anterior a la fecha de creación.");
        }

        Guid[] normalizedPatternIds = patternIds
            .Where(patternId => patternId != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedPatternIds.Length == 0)
        {
            throw new ArgumentException(
                "El incidente debe estar relacionado con al menos un patrón.",
                nameof(patternIds));
        }

        Id = id;
        Title = title.Trim();
        Description = description.Trim();
        Severity = severity;
        Status = status;
        Confidence = confidence;
        PatternIds = normalizedPatternIds;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ProbableCause = NormalizeOptional(probableCause);
        RecommendedAction = NormalizeOptional(recommendedAction);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
