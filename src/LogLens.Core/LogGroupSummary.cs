namespace LogLens.Core;

public sealed record LogGroupSummary
{
    public LogFingerprint Fingerprint { get; }

    public long OccurrenceCount { get; }

    public DateTimeOffset? FirstSeen { get; }

    public DateTimeOffset? LastSeen { get; }

    public LogLevel HighestLevel { get; }

    public string RepresentativeMessage { get; }

    public IReadOnlyList<string> Services { get; }

    public IReadOnlyList<string> ExceptionTypes { get; }

    public IReadOnlyList<int> StatusCodes { get; }

    public IReadOnlyList<LogGroupSample> Samples { get; }

    public bool IsRecurring =>
        OccurrenceCount > 1;

    public LogGroupSummary(
        LogFingerprint fingerprint,
        long occurrenceCount,
        DateTimeOffset? firstSeen,
        DateTimeOffset? lastSeen,
        LogLevel highestLevel,
        string representativeMessage,
        IEnumerable<string> services,
        IEnumerable<string> exceptionTypes,
        IEnumerable<int> statusCodes,
        IEnumerable<LogGroupSample> samples)
    {
        ArgumentNullException.ThrowIfNull(fingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            representativeMessage);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(exceptionTypes);
        ArgumentNullException.ThrowIfNull(statusCodes);
        ArgumentNullException.ThrowIfNull(samples);

        if (occurrenceCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrenceCount),
                "El número de apariciones debe ser mayor que cero.");
        }

        if (
            firstSeen.HasValue &&
            lastSeen.HasValue &&
            lastSeen.Value < firstSeen.Value)
        {
            throw new ArgumentException(
                "La última aparición no puede ser anterior a la primera.");
        }

        string[] serviceValues =
            NormalizeStrings(services);

        string[] exceptionValues =
            NormalizeStrings(exceptionTypes);

        int[] statusCodeValues =
            NormalizeStatusCodes(statusCodes);

        LogGroupSample[] sampleValues =
            samples.ToArray();

        if (sampleValues.Length > occurrenceCount)
        {
            throw new ArgumentException(
                "La cantidad de muestras no puede superar las apariciones.",
                nameof(samples));
        }

        Fingerprint = fingerprint;
        OccurrenceCount = occurrenceCount;
        FirstSeen = firstSeen;
        LastSeen = lastSeen;
        HighestLevel = highestLevel;
        RepresentativeMessage =
            representativeMessage.Trim();

        Services = serviceValues;
        ExceptionTypes = exceptionValues;
        StatusCodes = statusCodeValues;
        Samples = sampleValues;
    }

    private static string[] NormalizeStrings(
        IEnumerable<string> values)
    {
        return values
            .Where(value =>
                !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                value => value,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static int[] NormalizeStatusCodes(
        IEnumerable<int> values)
    {
        List<int> normalized = [];

        foreach (int value in values)
        {
            if (value is < 100 or > 599)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(values),
                    "Los códigos HTTP deben estar entre 100 y 599.");
            }

            normalized.Add(value);
        }

        return normalized
            .Distinct()
            .OrderBy(value => value)
            .ToArray();
    }
}