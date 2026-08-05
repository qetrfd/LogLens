using System.Globalization;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class CriticalIncidentDiagnosticRule
    : IIncidentDiagnosticRule
{
    public string Id =>
        "critical-log-level";

    public string Name =>
        "Critical log level";

    public int Order =>
        100;

    public IncidentDiagnosis? Evaluate(
        IncidentDiagnosticContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        LogGroupSummary group =
            context.Group;

        if (group.HighestLevel != LogLevel.Critical)
        {
            return null;
        }

        double confidence =
            Math.Min(
                100,
                92 +
                Math.Min(
                    8,
                    Math.Max(
                        0,
                        group.OccurrenceCount - 1) * 2));

        List<DiagnosticEvidence> evidence =
        [
            new(
                "highest-level",
                "Nivel más alto",
                group.HighestLevel.ToString()),

            new(
                "occurrence-count",
                "Apariciones",
                group.OccurrenceCount.ToString(
                    CultureInfo.InvariantCulture)),

            new(
                "group-percentage",
                "Porcentaje del archivo",
                $"{context.GroupPercentage:0.##}%"),

            new(
                "representative-message",
                "Mensaje representativo",
                group.RepresentativeMessage)
        ];

        if (group.ExceptionTypes.Count > 0)
        {
            evidence.Add(
                new DiagnosticEvidence(
                    "exception-types",
                    "Excepciones",
                    string.Join(
                        ", ",
                        group.ExceptionTypes)));
        }

        if (group.Services.Count > 0)
        {
            evidence.Add(
                new DiagnosticEvidence(
                    "affected-services",
                    "Servicios afectados",
                    string.Join(
                        ", ",
                        group.Services)));
        }

        string summary =
            group.OccurrenceCount == 1
                ? "Se detectó una entrada crítica que puede indicar una interrupción grave."
                : $"Se detectaron {group.OccurrenceCount:N0} entradas críticas equivalentes.";

        string[] actions =
        [
            "Revisar inmediatamente el mensaje y las excepciones asociadas.",
            "Confirmar si el servicio afectado continúa disponible.",
            "Buscar eventos anteriores relacionados con la misma huella.",
            "Revisar cambios recientes de configuración, despliegues o dependencias."
        ];

        return new IncidentDiagnosis(
            Id,
            "Incidente crítico detectado",
            summary,
            IncidentPriority.Critical,
            confidence,
            group.Fingerprint.Value,
            evidence,
            actions,
            context.AnalyzedAt);
    }
}