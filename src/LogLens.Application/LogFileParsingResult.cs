using LogLens.Core;

namespace LogLens.Application;

public sealed record LogFileParsingResult
{
    public Guid SourceId { get; }

    public string SourceName { get; }

    public string FilePath { get; }

    public long TotalLines { get; }

    public long ParsedLines { get; }

    public long UnparsedLines { get; }

    public IReadOnlyDictionary<LogLevel, long> LevelCounts { get; }

    public IReadOnlyDictionary<string, long> ParserCounts { get; }

    public IReadOnlyList<ParsedLogLine> Preview { get; }

    public DateTimeOffset CompletedAt { get; }

    public double ParsedPercentage =>
        TotalLines == 0
            ? 0
            : ParsedLines * 100d / TotalLines;

    public LogFileParsingResult(
        Guid sourceId,
        string sourceName,
        string filePath,
        long totalLines,
        long parsedLines,
        long unparsedLines,
        IReadOnlyDictionary<LogLevel, long> levelCounts,
        IReadOnlyDictionary<string, long> parserCounts,
        IEnumerable<ParsedLogLine> preview,
        DateTimeOffset completedAt)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la fuente no puede estar vacío.",
                nameof(sourceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(levelCounts);
        ArgumentNullException.ThrowIfNull(parserCounts);
        ArgumentNullException.ThrowIfNull(preview);

        if (totalLines < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalLines));
        }

        if (parsedLines < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parsedLines));
        }

        if (unparsedLines < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unparsedLines));
        }

        if (parsedLines + unparsedLines != totalLines)
        {
            throw new ArgumentException(
                "La suma de líneas procesadas y no procesadas debe coincidir con el total.");
        }

        Dictionary<LogLevel, long> levelCountsCopy = [];

        foreach (
            KeyValuePair<LogLevel, long> item in levelCounts)
        {
            if (item.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(levelCounts));
            }

            levelCountsCopy[item.Key] = item.Value;
        }

        Dictionary<string, long> parserCountsCopy =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (
            KeyValuePair<string, long> item in parserCounts)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                continue;
            }

            if (item.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(parserCounts));
            }

            parserCountsCopy[item.Key.Trim()] = item.Value;
        }

        SourceId = sourceId;
        SourceName = sourceName.Trim();
        FilePath = Path.GetFullPath(filePath.Trim());
        TotalLines = totalLines;
        ParsedLines = parsedLines;
        UnparsedLines = unparsedLines;
        LevelCounts = levelCountsCopy;
        ParserCounts = parserCountsCopy;
        Preview = preview.ToArray();
        CompletedAt = completedAt;
    }
}