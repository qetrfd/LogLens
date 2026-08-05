using System.Globalization;
using LogLens.Core;

namespace LogLens.Application;

public sealed class LogAnalysisExplorerService
{
    public IReadOnlyList<LogGroupSummary> QueryGroups(
        IEnumerable<LogGroupSummary> groups,
        LogGroupQueryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(groups);

        options ??=
            new LogGroupQueryOptions();

        IEnumerable<LogGroupSummary> query =
            groups;

        if (options.Level.HasValue)
        {
            query = query.Where(
                group =>
                    group.HighestLevel ==
                    options.Level.Value);
        }

        if (
            !string.IsNullOrWhiteSpace(
                options.SearchText))
        {
            query = query.Where(
                group =>
                    MatchesGroupSearch(
                        group,
                        options.SearchText));
        }

        return OrderGroups(
                query,
                options.SortOrder)
            .ToArray();
    }

    public IReadOnlyList<IncidentDiagnosis>
        QueryDiagnoses(
            IEnumerable<IncidentDiagnosis> diagnoses,
            IncidentDiagnosisQueryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(diagnoses);

        options ??=
            new IncidentDiagnosisQueryOptions();

        IEnumerable<IncidentDiagnosis> query =
            diagnoses;

        if (options.Priority.HasValue)
        {
            query = query.Where(
                diagnosis =>
                    diagnosis.Priority ==
                    options.Priority.Value);
        }

        if (
            !string.IsNullOrWhiteSpace(
                options.SearchText))
        {
            query = query.Where(
                diagnosis =>
                    MatchesDiagnosisSearch(
                        diagnosis,
                        options.SearchText));
        }

        return OrderDiagnoses(
                query,
                options.SortOrder)
            .ToArray();
    }

    private static IEnumerable<LogGroupSummary>
        OrderGroups(
            IEnumerable<LogGroupSummary> groups,
            LogGroupSortOrder sortOrder)
    {
        return sortOrder switch
        {
            LogGroupSortOrder.Frequency =>
                groups
                    .OrderByDescending(
                        group =>
                            group.OccurrenceCount)
                    .ThenByDescending(
                        group =>
                            GetLogLevelWeight(
                                group.HighestLevel))
                    .ThenBy(
                        group =>
                            group.RepresentativeMessage,
                        StringComparer.OrdinalIgnoreCase),

            LogGroupSortOrder.Newest =>
                groups
                    .OrderByDescending(
                        group =>
                            group.LastSeen)
                    .ThenByDescending(
                        group =>
                            group.OccurrenceCount),

            LogGroupSortOrder.Oldest =>
                groups
                    .OrderBy(
                        group =>
                            group.FirstSeen)
                    .ThenBy(
                        group =>
                            group.RepresentativeMessage,
                        StringComparer.OrdinalIgnoreCase),

            LogGroupSortOrder.Message =>
                groups
                    .OrderBy(
                        group =>
                            group.RepresentativeMessage,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(
                        group =>
                            group.OccurrenceCount),

            _ =>
                groups
                    .OrderByDescending(
                        group =>
                            GetLogLevelWeight(
                                group.HighestLevel))
                    .ThenByDescending(
                        group =>
                            group.OccurrenceCount)
                    .ThenByDescending(
                        group =>
                            group.LastSeen)
        };
    }

    private static IEnumerable<IncidentDiagnosis>
        OrderDiagnoses(
            IEnumerable<IncidentDiagnosis> diagnoses,
            IncidentDiagnosisSortOrder sortOrder)
    {
        return sortOrder switch
        {
            IncidentDiagnosisSortOrder.Confidence =>
                diagnoses
                    .OrderByDescending(
                        diagnosis =>
                            diagnosis.ConfidencePercentage)
                    .ThenByDescending(
                        diagnosis =>
                            GetPriorityWeight(
                                diagnosis.Priority))
                    .ThenBy(
                        diagnosis =>
                            diagnosis.Title,
                        StringComparer.OrdinalIgnoreCase),

            IncidentDiagnosisSortOrder.Newest =>
                diagnoses
                    .OrderByDescending(
                        diagnosis =>
                            diagnosis.DetectedAt)
                    .ThenByDescending(
                        diagnosis =>
                            GetPriorityWeight(
                                diagnosis.Priority)),

            IncidentDiagnosisSortOrder.Title =>
                diagnoses
                    .OrderBy(
                        diagnosis =>
                            diagnosis.Title,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(
                        diagnosis =>
                            GetPriorityWeight(
                                diagnosis.Priority)),

            _ =>
                diagnoses
                    .OrderByDescending(
                        diagnosis =>
                            GetPriorityWeight(
                                diagnosis.Priority))
                    .ThenByDescending(
                        diagnosis =>
                            diagnosis.ConfidencePercentage)
                    .ThenBy(
                        diagnosis =>
                            diagnosis.Title,
                        StringComparer.OrdinalIgnoreCase)
        };
    }

    private static bool MatchesGroupSearch(
        LogGroupSummary group,
        string searchText)
    {
        if (
            Contains(
                group.RepresentativeMessage,
                searchText) ||
            Contains(
                group.Fingerprint.Value,
                searchText) ||
            Contains(
                group.Fingerprint.NormalizedMessage,
                searchText))
        {
            return true;
        }

        if (
            group.Services.Any(
                service =>
                    Contains(
                        service,
                        searchText)))
        {
            return true;
        }

        if (
            group.ExceptionTypes.Any(
                exceptionType =>
                    Contains(
                        exceptionType,
                        searchText)))
        {
            return true;
        }

        if (
            group.StatusCodes.Any(
                statusCode =>
                    Contains(
                        statusCode.ToString(
                            CultureInfo.InvariantCulture),
                        searchText)))
        {
            return true;
        }

        return group.Samples.Any(
            sample =>
                Contains(
                    sample.Message,
                    searchText) ||
                Contains(
                    sample.Service,
                    searchText) ||
                Contains(
                    sample.ExceptionType,
                    searchText) ||
                (
                    sample.StatusCode.HasValue &&
                    Contains(
                        sample.StatusCode.Value.ToString(
                            CultureInfo.InvariantCulture),
                        searchText)
                ));
    }

    private static bool MatchesDiagnosisSearch(
        IncidentDiagnosis diagnosis,
        string searchText)
    {
        if (
            Contains(
                diagnosis.Title,
                searchText) ||
            Contains(
                diagnosis.Summary,
                searchText) ||
            Contains(
                diagnosis.RuleId,
                searchText) ||
            Contains(
                diagnosis.Fingerprint,
                searchText))
        {
            return true;
        }

        if (
            diagnosis.Evidence.Any(
                evidence =>
                    Contains(
                        evidence.Code,
                        searchText) ||
                    Contains(
                        evidence.Label,
                        searchText) ||
                    Contains(
                        evidence.Value,
                        searchText)))
        {
            return true;
        }

        return diagnosis.RecommendedActions.Any(
            action =>
                Contains(
                    action,
                    searchText));
    }

    private static bool Contains(
        string? value,
        string searchText)
    {
        return
            !string.IsNullOrWhiteSpace(value) &&
            value.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase);
    }

    private static int GetLogLevelWeight(
        LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => 6,
            LogLevel.Error => 5,
            LogLevel.Warning => 4,
            LogLevel.Information => 3,
            LogLevel.Debug => 2,
            LogLevel.Trace => 1,
            _ => 0
        };
    }

    private static int GetPriorityWeight(
        IncidentPriority priority)
    {
        return priority switch
        {
            IncidentPriority.Critical => 4,
            IncidentPriority.High => 3,
            IncidentPriority.Medium => 2,
            IncidentPriority.Low => 1,
            _ => 0
        };
    }
}