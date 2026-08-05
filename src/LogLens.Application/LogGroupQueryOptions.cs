using LogLens.Core;

namespace LogLens.Application;

public sealed record LogGroupQueryOptions
{
    public string SearchText { get; }

    public LogLevel? Level { get; }

    public LogGroupSortOrder SortOrder { get; }

    public LogGroupQueryOptions(
        string? searchText = null,
        LogLevel? level = null,
        LogGroupSortOrder sortOrder =
            LogGroupSortOrder.Severity)
    {
        SearchText =
            string.IsNullOrWhiteSpace(searchText)
                ? string.Empty
                : searchText.Trim();

        Level = level;
        SortOrder = sortOrder;
    }
}