using LogLens.Core;

namespace LogLens.Application;

public sealed record IncidentDiagnosisQueryOptions
{
    public string SearchText { get; }

    public IncidentPriority? Priority { get; }

    public IncidentDiagnosisSortOrder SortOrder { get; }

    public IncidentDiagnosisQueryOptions(
        string? searchText = null,
        IncidentPriority? priority = null,
        IncidentDiagnosisSortOrder sortOrder =
            IncidentDiagnosisSortOrder.Priority)
    {
        SearchText =
            string.IsNullOrWhiteSpace(searchText)
                ? string.Empty
                : searchText.Trim();

        Priority = priority;
        SortOrder = sortOrder;
    }
}