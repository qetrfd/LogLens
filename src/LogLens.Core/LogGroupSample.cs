namespace LogLens.Core;

public sealed record LogGroupSample
{
    public Guid SourceId { get; }

    public long LineNumber { get; }

    public DateTimeOffset? Timestamp { get; }

    public LogLevel Level { get; }

    public string Message { get; }

    public string? Service { get; }

    public string? ExceptionType { get; }

    public int? StatusCode { get; }

    public LogGroupSample(
        Guid sourceId,
        long lineNumber,
        DateTimeOffset? timestamp,
        LogLevel level,
        string message,
        string? service = null,
        string? exceptionType = null,
        int? statusCode = null)
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

        if (statusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                "El código HTTP debe estar entre 100 y 599.");
        }

        SourceId = sourceId;
        LineNumber = lineNumber;
        Timestamp = timestamp;
        Level = level;
        Message = message.Trim();
        Service = NormalizeOptional(service);
        ExceptionType = NormalizeOptional(exceptionType);
        StatusCode = statusCode;
    }

    public static LogGroupSample From(
        ParsedLogLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new LogGroupSample(
            line.SourceId,
            line.LineNumber,
            line.Timestamp,
            line.Level,
            line.Message,
            line.Service,
            line.ExceptionType,
            line.StatusCode);
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}