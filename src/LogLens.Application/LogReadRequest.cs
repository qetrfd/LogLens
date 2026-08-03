namespace LogLens.Application;

public sealed record LogReadRequest
{
    public const int DefaultMaximumLineLength = 1_048_576;

    public const int DefaultProgressIntervalLines = 500;

    public Guid SourceId { get; }

    public string FilePath { get; }

    public int MaximumLineLength { get; }

    public int ProgressIntervalLines { get; }

    public LogReadRequest(
        Guid sourceId,
        string filePath,
        int maximumLineLength = DefaultMaximumLineLength,
        int progressIntervalLines = DefaultProgressIntervalLines)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(sourceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (maximumLineLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumLineLength),
                "La longitud máxima debe ser mayor que cero.");
        }

        if (progressIntervalLines < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(progressIntervalLines),
                "El intervalo de progreso debe ser mayor que cero.");
        }

        SourceId = sourceId;
        FilePath = Path.GetFullPath(filePath.Trim());
        MaximumLineLength = maximumLineLength;
        ProgressIntervalLines = progressIntervalLines;
    }
}
