using System.Globalization;
using System.Text.RegularExpressions;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class HighLatencyDiagnosticRule
    : IIncidentDiagnosticRule
{
    private static readonly Regex DurationPattern = new(
        @"(?<value>\d+(?:\.\d+)?)\s*(?<unit>ms|milliseconds?|s|seconds?)\b",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase);

    private static readonly string[] Keywords =
    [
        "latency",
        "latencia",
        "slow",
        "lento",
        "timeout",
        "timed out",
        "response time",
        "tiempo de respuesta",
        "elapsed",
        "took"
    ];

    public string Id =>
        "high-latency";

    public string Name =>
        "High latency";

    public int Order =>
        400;

    public IncidentDiagnosis? Evaluate(
        IncidentDiagnosticContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        LogGroupSummary group =
            context.Group;

        string combinedText =
            BuildCombinedText(group);

        bool containsLatencyIndicator =
            Keywords.Any(
                keyword =>
                    combinedText.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase));

        if (!containsLatencyIndicator)
        {
            return null;
        }

        double[] durations =
            ExtractDurations(group)
                .ToArray();

        double? maximumDuration =
            durations.Length == 0
                ? null
                : durations.Max();

        IncidentPriority priority =
            DeterminePriority(
                group,
                maximumDuration);

        double confidence =
            CalculateConfidence(
                group,
                maximumDuration);

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
                "normalized-message",
                "Mensaje normalizado",
                group.Fingerprint.NormalizedMessage)
        ];

        if (maximumDuration.HasValue)
        {
            evidence.Add(
                new DiagnosticEvidence(
                    "maximum-duration",
                    "Duración máxima detectada",
                    FormatDuration(
                        maximumDuration.Value)));
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
            maximumDuration.HasValue
                ? $"Se detectaron eventos de latencia con una duración máxima de " +
                  $"{FormatDuration(maximumDuration.Value)}."
                : "Se detectaron mensajes relacionados con latencia, lentitud o tiempos de espera.";

        string[] actions =
        [
            "Revisar el tiempo de respuesta de las dependencias involucradas.",
            "Comparar la latencia con métricas de CPU, memoria, red y base de datos.",
            "Identificar consultas, endpoints o procesos lentos.",
            "Revisar límites de tiempo, reintentos y saturación de conexiones."
        ];

        return new IncidentDiagnosis(
            Id,
            "Latencia elevada detectada",
            summary,
            priority,
            confidence,
            group.Fingerprint.Value,
            evidence,
            actions,
            context.AnalyzedAt);
    }

    private static string BuildCombinedText(
        LogGroupSummary group)
    {
        List<string> values =
        [
            group.RepresentativeMessage,
            group.Fingerprint.NormalizedMessage
        ];

        values.AddRange(
            group.Samples.Select(
                sample => sample.Message));

        return string.Join(
            " ",
            values);
    }

    private static IEnumerable<double>
        ExtractDurations(
            LogGroupSummary group)
    {
        List<string> messages =
        [
            group.RepresentativeMessage
        ];

        messages.AddRange(
            group.Samples.Select(
                sample => sample.Message));

        foreach (string message in messages)
        {
            foreach (
                Match match
                in DurationPattern.Matches(message))
            {
                if (
                    !double.TryParse(
                        match.Groups["value"].Value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                {
                    continue;
                }

                string unit =
                    match.Groups["unit"].Value;

                if (
                    string.Equals(
                        unit,
                        "s",
                        StringComparison.OrdinalIgnoreCase) ||
                    unit.StartsWith(
                        "second",
                        StringComparison.OrdinalIgnoreCase))
                {
                    value *= 1000;
                }

                yield return value;
            }
        }
    }

    private static IncidentPriority DeterminePriority(
        LogGroupSummary group,
        double? maximumDuration)
    {
        if (group.HighestLevel == LogLevel.Critical)
        {
            return IncidentPriority.Critical;
        }

        if (
            maximumDuration >= 2000 ||
            group.OccurrenceCount >= 5)
        {
            return IncidentPriority.High;
        }

        return IncidentPriority.Medium;
    }

    private static double CalculateConfidence(
        LogGroupSummary group,
        double? maximumDuration)
    {
        double confidence = 68;

        if (maximumDuration.HasValue)
        {
            confidence += 15;
        }

        if (group.IsRecurring)
        {
            confidence += 8;
        }

        if (maximumDuration >= 1000)
        {
            confidence += 5;
        }

        return Math.Min(
            100,
            confidence);
    }

    private static string FormatDuration(
        double milliseconds)
    {
        if (milliseconds >= 1000)
        {
            return
                $"{milliseconds / 1000:0.##} segundos";
        }

        return
            $"{milliseconds:0.##} ms";
    }
}