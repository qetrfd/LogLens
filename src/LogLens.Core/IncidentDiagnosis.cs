namespace LogLens.Core;

public sealed record IncidentDiagnosis
{
    public string RuleId { get; }

    public string Title { get; }

    public string Summary { get; }

    public IncidentPriority Priority { get; }

    public double ConfidencePercentage { get; }

    public string Fingerprint { get; }

    public IReadOnlyList<DiagnosticEvidence> Evidence { get; }

    public IReadOnlyList<string> RecommendedActions { get; }

    public DateTimeOffset DetectedAt { get; }

    public bool RequiresImmediateAttention =>
        Priority is IncidentPriority.High
            or IncidentPriority.Critical;

    public IncidentDiagnosis(
        string ruleId,
        string title,
        string summary,
        IncidentPriority priority,
        double confidencePercentage,
        string fingerprint,
        IEnumerable<DiagnosticEvidence> evidence,
        IEnumerable<string> recommendedActions,
        DateTimeOffset detectedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(
            recommendedActions);

        if (
            confidencePercentage < 0 ||
            confidencePercentage > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidencePercentage),
                "La confianza debe estar entre 0 y 100.");
        }

        DiagnosticEvidence[] evidenceValues =
            evidence
                .Where(item => item is not null)
                .Distinct()
                .ToArray();

        string[] actionValues =
            recommendedActions
                .Where(action =>
                    !string.IsNullOrWhiteSpace(action))
                .Select(action => action.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (evidenceValues.Length == 0)
        {
            throw new ArgumentException(
                "El diagnóstico debe contener evidencia.",
                nameof(evidence));
        }

        if (actionValues.Length == 0)
        {
            throw new ArgumentException(
                "El diagnóstico debe contener al menos una acción recomendada.",
                nameof(recommendedActions));
        }

        RuleId = NormalizeIdentifier(ruleId);
        Title = title.Trim();
        Summary = summary.Trim();
        Priority = priority;
        ConfidencePercentage = confidencePercentage;
        Fingerprint = fingerprint.Trim().ToLowerInvariant();
        Evidence = evidenceValues;
        RecommendedActions = actionValues;
        DetectedAt = detectedAt;
    }

    private static string NormalizeIdentifier(
        string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-');
    }
}