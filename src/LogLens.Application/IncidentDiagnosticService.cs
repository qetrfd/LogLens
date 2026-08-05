using LogLens.Core;

namespace LogLens.Application;

public sealed class IncidentDiagnosticService
{
    private readonly IReadOnlyList<IIncidentDiagnosticRule>
        _rules;

    public IncidentDiagnosticService(
        IEnumerable<IIncidentDiagnosticRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        IIncidentDiagnosticRule[] ruleValues =
            rules
                .Where(rule => rule is not null)
                .OrderBy(rule => rule.Order)
                .ThenBy(
                    rule => rule.Id,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (ruleValues.Length == 0)
        {
            throw new ArgumentException(
                "Debe proporcionarse al menos una regla de diagnóstico.",
                nameof(rules));
        }

        string? duplicateRuleId =
            ruleValues
                .GroupBy(
                    rule => rule.Id,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

        if (duplicateRuleId is not null)
        {
            throw new ArgumentException(
                $"El identificador de regla está duplicado: {duplicateRuleId}",
                nameof(rules));
        }

        _rules = ruleValues;
    }

    public IncidentDiagnosticResult Diagnose(
        LogGroupingResult groupingResult,
        DateTimeOffset? analyzedAt = null)
    {
        ArgumentNullException.ThrowIfNull(
            groupingResult);

        DateTimeOffset effectiveAnalyzedAt =
            analyzedAt ?? DateTimeOffset.UtcNow;

        if (groupingResult.GroupCount == 0)
        {
            return new IncidentDiagnosticResult(
                0,
                [],
                effectiveAnalyzedAt);
        }

        List<IncidentDiagnosis> diagnoses = [];

        HashSet<string> diagnosisKeys =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (
            LogGroupSummary group
            in groupingResult.Groups)
        {
            IncidentDiagnosticContext context = new(
                group,
                groupingResult.GroupedEntries,
                groupingResult.GroupCount,
                effectiveAnalyzedAt);

            foreach (
                IIncidentDiagnosticRule rule
                in _rules)
            {
                IncidentDiagnosis? diagnosis =
                    rule.Evaluate(context);

                if (diagnosis is null)
                {
                    continue;
                }

                string diagnosisKey =
                    $"{diagnosis.RuleId}|" +
                    $"{diagnosis.Fingerprint}";

                if (!diagnosisKeys.Add(diagnosisKey))
                {
                    continue;
                }

                diagnoses.Add(diagnosis);
            }
        }

        IncidentDiagnosis[] orderedDiagnoses =
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
                .ToArray();

        return new IncidentDiagnosticResult(
            groupingResult.GroupCount,
            orderedDiagnoses,
            effectiveAnalyzedAt);
    }

    private static int GetPriorityWeight(
        IncidentPriority priority)
    {
        return priority switch
        {
            IncidentPriority.Low => 1,
            IncidentPriority.Medium => 2,
            IncidentPriority.High => 3,
            IncidentPriority.Critical => 4,
            _ => 0
        };
    }
}