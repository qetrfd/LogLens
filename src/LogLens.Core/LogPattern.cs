namespace LogLens.Core;

public sealed record LogPattern
{
    public Guid Id { get; }

    public string Fingerprint { get; }

    public string RepresentativeMessage { get; }

    public LogLevel HighestLevel { get; }

    public int Occurrences { get; }

    public DateTimeOffset? FirstSeen { get; }

    public DateTimeOffset? LastSeen { get; }

    public IReadOnlyList<string> Samples { get; }

    public LogPattern(
        Guid id,
        string fingerprint,
        string representativeMessage,
        LogLevel highestLevel,
        int occurrences,
        DateTimeOffset? firstSeen,
        DateTimeOffset? lastSeen,
        IEnumerable<string>? samples = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del patrón no puede estar vacío.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(representativeMessage);

        if (occurrences < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrences),
                "Un patrón debe tener al menos una aparición.");
        }

        if (
            firstSeen.HasValue &&
            lastSeen.HasValue &&
            lastSeen.Value < firstSeen.Value)
        {
            throw new ArgumentException(
                "La última aparición no puede ser anterior a la primera.");
        }

        Id = id;
        Fingerprint = fingerprint.Trim();
        RepresentativeMessage = representativeMessage.Trim();
        HighestLevel = highestLevel;
        Occurrences = occurrences;
        FirstSeen = firstSeen;
        LastSeen = lastSeen;
        Samples = samples?
            .Where(sample => !string.IsNullOrWhiteSpace(sample))
            .Select(sample => sample.Trim())
            .Distinct(StringComparer.Ordinal)
            .Take(5)
            .ToArray()
            ?? [];
    }
}
