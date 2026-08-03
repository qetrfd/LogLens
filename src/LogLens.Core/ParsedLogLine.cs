namespace LogLens.Core;

public sealed record ParsedLogLine
{
    public Guid SourceId { get; }

    public long LineNumber { get; }

    public DateTimeOffset? Timestamp { get; }

    public LogLevel Level { get; }

    public string Message { get; }

    public string RawText { get; }

    public string? Service { get; }

    public string? ExceptionType { get; }

    public int? StatusCode { get; }

    public long? DurationMilliseconds { get; }

    public string? CorrelationId { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public ParsedLogLine(
        Guid sourceId,
        long lineNumber,
        DateTimeOffset? timestamp,
        LogLevel level,
        string message,
        string rawText,
        string? service = null,
        string? exceptionType = null,
        int? statusCode = null,
        long? durationMilliseconds = null,
        string? correlationId = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(sourceId));
        }

        if (lineNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineNumber),
                "El número de línea debe ser mayor que cero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawText);

        if (statusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                "El código HTTP debe estar entre 100 y 599.");
        }

        if (durationMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMilliseconds),
                "La duración no puede ser negativa.");
        }

        Dictionary<string, string> metadataCopy =
            new(StringComparer.OrdinalIgnoreCase);

        if (metadata is not null)
        {
            foreach (KeyValuePair<string, string> item in metadata)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    continue;
                }

                metadataCopy[item.Key.Trim()] = item.Value;
            }
        }

        SourceId = sourceId;
        LineNumber = lineNumber;
        Timestamp = timestamp;
        Level = level;
        Message = message.Trim();
        RawText = rawText;
        Service = NormalizeOptional(service);
        ExceptionType = NormalizeOptional(exceptionType);
        StatusCode = statusCode;
        DurationMilliseconds = durationMilliseconds;
        CorrelationId = NormalizeOptional(correlationId);
        Metadata = metadataCopy;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}