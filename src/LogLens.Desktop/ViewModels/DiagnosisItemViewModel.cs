using System.Globalization;
using LogLens.Core;

namespace LogLens.Desktop.ViewModels;

public sealed class DiagnosisItemViewModel
{
    public IncidentDiagnosis Model { get; }

    public IncidentPriority Priority =>
        Model.Priority;

    public string PriorityText =>
        Model.Priority switch
        {
            IncidentPriority.Critical =>
                "Crítica",

            IncidentPriority.High =>
                "Alta",

            IncidentPriority.Medium =>
                "Media",

            IncidentPriority.Low =>
                "Baja",

            _ =>
                "Sin prioridad"
        };

    public string Title =>
        Model.Title;

    public string Summary =>
        Model.Summary;

    public string RuleId =>
        Model.RuleId;

    public string ConfidenceText =>
        $"{Model.ConfidencePercentage:0.##}%";

    public string Fingerprint =>
        Model.Fingerprint;

    public string ShortFingerprint =>
        Shorten(
            Model.Fingerprint,
            18);

    public string DetectedAtText =>
        Model.DetectedAt.ToString(
            "yyyy-MM-dd HH:mm:ss zzz",
            CultureInfo.InvariantCulture);

    public bool RequiresImmediateAttention =>
        Model.RequiresImmediateAttention;

    public IReadOnlyList<DiagnosticEvidence>
        Evidence =>
            Model.Evidence;

    public IReadOnlyList<string>
        RecommendedActions =>
            Model.RecommendedActions;

    public string ActionsText =>
        string.Join(
            Environment.NewLine,
            Model.RecommendedActions.Select(
                (action, index) =>
                    $"{index + 1}. {action}"));

    public string CompleteText
    {
        get
        {
            string evidenceText =
                string.Join(
                    Environment.NewLine,
                    Model.Evidence.Select(
                        evidence =>
                            $"- {evidence.Label}: " +
                            $"{evidence.Value}"));

            return string.Join(
                Environment.NewLine,
                [
                    $"Título: {Title}",
                    $"Regla: {RuleId}",
                    $"Prioridad: {PriorityText}",
                    $"Confianza: {ConfidenceText}",
                    $"Detectado: {DetectedAtText}",
                    $"Resumen: {Summary}",
                    $"Huella: {Fingerprint}",
                    string.Empty,
                    "Evidencia:",
                    evidenceText,
                    string.Empty,
                    "Acciones recomendadas:",
                    ActionsText
                ]);
        }
    }

    public DiagnosisItemViewModel(
        IncidentDiagnosis model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Model = model;
    }

    private static string Shorten(
        string value,
        int maximumLength)
    {
        if (value.Length <= maximumLength)
        {
            return value;
        }

        return value[..maximumLength];
    }
}