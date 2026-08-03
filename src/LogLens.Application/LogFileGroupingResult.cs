using LogLens.Core;

namespace LogLens.Application;

public sealed record LogFileGroupingResult
{
    public Guid SourceId { get; }

    public string SourceName { get; }

    public string FilePath { get; }

    public long TotalLines { get; }

    public long ParsedLines { get; }

    public long UnparsedLines { get; }

    public LogGroupingResult Grouping { get; }

    public IReadOnlyList<LogGroupSummary> Groups =>
        Grouping.Groups;

    public int GroupCount =>
        Grouping.GroupCount;

    public int RecurringGroupCount =>
        Grouping.RecurringGroupCount;

    public double ParsedPercentage =>
        TotalLines == 0
            ? 0
            : ParsedLines * 100d / TotalLines;

    public double RecurringEntryPercentage =>
        Grouping.GroupedEntries == 0
            ? 0
            : Groups
                .Where(group => group.IsRecurring)
                .Sum(group => group.OccurrenceCount)
                * 100d
                / Grouping.GroupedEntries;

    public DateTimeOffset CompletedAt { get; }

    public LogFileGroupingResult(
        Guid sourceId,
        string sourceName,
        string filePath,
        long totalLines,
        long parsedLines,
        long unparsedLines,
        LogGroupingResult grouping,
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
        ArgumentNullException.ThrowIfNull(grouping);

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

        if (grouping.TotalEntries != parsedLines)
        {
            throw new ArgumentException(
                "Las entradas del agrupamiento deben coincidir con las líneas procesadas.",
                nameof(grouping));
        }

        SourceId = sourceId;
        SourceName = sourceName.Trim();
        FilePath = Path.GetFullPath(filePath.Trim());
        TotalLines = totalLines;
        ParsedLines = parsedLines;
        UnparsedLines = unparsedLines;
        Grouping = grouping;
        CompletedAt = completedAt;
    }
}