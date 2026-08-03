using LogLens.Core;

namespace LogLens.Application;

public sealed record LogFileInspectionResult
{
    public Guid SourceId { get; }

    public string FilePath { get; }

    public long TotalLines { get; }

    public long EmptyLines { get; }

    public int LongestLineLength { get; }

    public IReadOnlyList<RawLogLine> Preview { get; }

    public DateTimeOffset CompletedAt { get; }

    public LogFileInspectionResult(
        Guid sourceId,
        string filePath,
        long totalLines,
        long emptyLines,
        int longestLineLength,
        IEnumerable<RawLogLine> preview,
        DateTimeOffset completedAt)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(sourceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(preview);

        if (totalLines < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalLines));
        }

        if (emptyLines < 0 || emptyLines > totalLines)
        {
            throw new ArgumentOutOfRangeException(nameof(emptyLines));
        }

        if (longestLineLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longestLineLength));
        }

        SourceId = sourceId;
        FilePath = filePath;
        TotalLines = totalLines;
        EmptyLines = emptyLines;
        LongestLineLength = longestLineLength;
        Preview = preview.ToArray();
        CompletedAt = completedAt;
    }
}
