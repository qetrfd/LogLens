using System.Globalization;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class RecurringFailureDiagnosticRule
    : IIncidentDiagnosticRule
{
    public string Id =>
        "recurring-failure";

    public string Name =>
        "Recurring failure";

    public int Order =>
        200;

    public IncidentDiagnosis? Evaluate(
        IncidentDiagnosticContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        LogGroupSummary group =
            context.Group;

        bool isFailureLevel =
            group.HighestLevel is
                LogLevel.Error or
                LogLevel.Critical;

        if (
            !group.IsRecurring ||
            !isFailureLevel)
        {
            return null;
        }

        IncidentPriority priority =
            DeterminePriority(
                group,
                context.GroupPercentage);

        double confidence =
            CalculateConfidence(
                group.OccurrenceCount,
                context.GroupPercentage);

        List<DiagnosticEvidence> evidence =
        [
            new(
                "occurrence-count",
                "Apariciones",
                group.OccurrenceCount.ToString(
                    CultureInfo.InvariantCulture)),

            new(
                "group-percentage",
                "Porcentaje de entradas",
                $"{context.GroupPercentage:0.##}%"),

            new(
                "highest-level",
                "Nivel más alto",
                group.HighestLevel.ToString()),

            new(
                "normalized-message",
                "Mensaje normalizado",
                group.Fingerprint.NormalizedMessage)
        ];

        if (
            group.FirstSeen.HasValue &&
            group.LastSeen.HasValue)
        {
            TimeSpan duration =
                group.LastSeen.Value -
                group.FirstSeen.Value;

            evidence.Add(
                new DiagnosticEvidence(
                    "activity-window",
                    "Ventana de actividad",
                    FormatDuration(duration)));
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

        if (group.StatusCodes.Count > 0)
        {
            evidence.Add(
                new DiagnosticEvidence(
                    "http-status-codes",
                    "Códigos HTTP",
                    string.Join(
                        ", ",
                        group.StatusCodes)));
        }

        string summary =
            $"El mismo fallo apareció " +
            $"{group.OccurrenceCount:N0} veces y representa " +
            $"{context.GroupPercentage:0.##}% de las entradas agrupadas.";

        string[] actions =
        [
            "Revisar la primera aparición para identificar el evento que inició el fallo.",
            "Comparar las muestras del grupo para confirmar que comparten la misma causa.",
            "Verificar la disponibilidad de servicios, red, base de datos y dependencias externas.",
            "Aplicar una corrección y confirmar que la frecuencia del grupo disminuya."
        ];

        return new IncidentDiagnosis(
            Id,
            "Fallo recurrente detectado",
            summary,
            priority,
            confidence,
            group.Fingerprint.Value,
            evidence,
            actions,
            context.AnalyzedAt);
    }

    private static IncidentPriority DeterminePriority(
        LogGroupSummary group,
        double groupPercentage)
    {
        if (
            group.HighestLevel == LogLevel.Critical &&
            group.OccurrenceCount >= 5)
        {
            return IncidentPriority.Critical;
        }

        if (
            group.OccurrenceCount >= 5 ||
            groupPercentage >= 20)
        {
            return IncidentPriority.High;
        }

        return IncidentPriority.Medium;
    }

    private static double CalculateConfidence(
        long occurrenceCount,
        double groupPercentage)
    {
        double occurrenceScore =
            Math.Min(
                20,
                Math.Max(
                    0,
                    occurrenceCount - 2) * 4);

        double percentageScore =
            Math.Min(
                10,
                groupPercentage / 5);

        return Math.Min(
            100,
            70 +
            occurrenceScore +
            percentageScore);
    }

    private static string FormatDuration(
        TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
        {
            return $"{duration.TotalDays:0.##} días";
        }

        if (duration.TotalHours >= 1)
        {
            return $"{duration.TotalHours:0.##} horas";
        }

        if (duration.TotalMinutes >= 1)
        {
            return $"{duration.TotalMinutes:0.##} minutos";
        }

        return $"{duration.TotalSeconds:0.##} segundos";
    }
}