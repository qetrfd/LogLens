using LogLens.Core;

namespace LogLens.Application;

public sealed record LogGroupingResult
{
    public long TotalEntries { get; }

    public long GroupedEntries { get; }

    public long ExcludedEntries { get; }

    public IReadOnlyList<LogGroupSummary> Groups { get; }

    public DateTimeOffset CompletedAt { get; }

    public int GroupCount =>
        Groups.Count;

    public int RecurringGroupCount =>
        Groups.Count(group => group.IsRecurring);

    public double AverageOccurrencesPerGroup =>
        GroupCount == 0
            ? 0
            : GroupedEntries / (double)GroupCount;

    public LogGroupingResult(
        long totalEntries,
        long groupedEntries,
        long excludedEntries,
        IEnumerable<LogGroupSummary> groups,
        DateTimeOffset completedAt)
    {
        ArgumentNullException.ThrowIfNull(groups);

        if (totalEntries < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalEntries));
        }

        if (groupedEntries < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(groupedEntries));
        }

        if (excludedEntries < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(excludedEntries));
        }

        if (groupedEntries + excludedEntries != totalEntries)
        {
            throw new ArgumentException(
                "La suma de entradas agrupadas y excluidas debe coincidir con el total.");
        }

        LogGroupSummary[] groupValues =
            groups.ToArray();

        long groupedOccurrences =
            groupValues.Sum(
                group => group.OccurrenceCount);

        if (groupedOccurrences != groupedEntries)
        {
            throw new ArgumentException(
                "Las apariciones de los grupos deben coincidir con las entradas agrupadas.",
                nameof(groups));
        }

        TotalEntries = totalEntries;
        GroupedEntries = groupedEntries;
        ExcludedEntries = excludedEntries;
        Groups = groupValues;
        CompletedAt = completedAt;
    }
}