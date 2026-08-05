using System.Globalization;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class ConnectionFailureDiagnosticRule
    : IIncidentDiagnosticRule
{
    private static readonly string[] MessageIndicators =
    [
        "connection refused",
        "connection failed",
        "connection reset",
        "could not connect",
        "unable to connect",
        "network unreachable",
        "host unreachable",
        "connection closed",
        "broken pipe",
        "socket error"
    ];

    private static readonly string[] ExceptionIndicators =
    [
        "SocketException",
        "HttpRequestException",
        "NetworkException",
        "ConnectException"
    ];

    public string Id =>
        "connection-failure";

    public string Name =>
        "Connection failure";

    public int Order =>
        500;

    public IncidentDiagnosis? Evaluate(
        IncidentDiagnosticContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        LogGroupSummary group =
            context.Group;

        string[] indicators =
            FindIndicators(group);

        if (indicators.Length == 0)
        {
            return null;
        }

        IncidentPriority priority =
            DeterminePriority(group);

        double confidence =
            CalculateConfidence(
                group,
                indicators.Length);

        List<DiagnosticEvidence> evidence =
        [
            new(
                "connection-indicators",
                "Indicadores detectados",
                string.Join(
                    ", ",
                    indicators)),

            new(
                "occurrence-count",
                "Apariciones",
                group.OccurrenceCount.ToString(
                    CultureInfo.InvariantCulture)),

            new(
                "highest-level",
                "Nivel más alto",
                group.HighestLevel.ToString()),

            new(
                "normalized-message",
                "Mensaje normalizado",
                group.Fingerprint.NormalizedMessage)
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
            group.IsRecurring
                ? $"El fallo de conexión apareció {group.OccurrenceCount:N0} veces."
                : "Se detectó un fallo relacionado con una conexión de red o servicio.";

        string[] actions =
        [
            "Confirmar que el host, puerto y servicio de destino estén disponibles.",
            "Revisar DNS, firewall, proxy, certificados y reglas de red.",
            "Verificar límites y saturación del conjunto de conexiones.",
            "Comprobar si la dependencia fue reiniciada o desplegada recientemente.",
            "Revisar reintentos y tiempos de espera para evitar fallos en cascada."
        ];

        return new IncidentDiagnosis(
            Id,
            "Fallo de conexión detectado",
            summary,
            priority,
            confidence,
            group.Fingerprint.Value,
            evidence,
            actions,
            context.AnalyzedAt);
    }

    private static string[] FindIndicators(
        LogGroupSummary group)
    {
        HashSet<string> indicators =
            new(StringComparer.OrdinalIgnoreCase);

        string text =
            $"{group.RepresentativeMessage} " +
            $"{group.Fingerprint.NormalizedMessage}";

        foreach (string indicator in MessageIndicators)
        {
            if (
                text.Contains(
                    indicator,
                    StringComparison.OrdinalIgnoreCase))
            {
                indicators.Add(indicator);
            }
        }

        foreach (string exceptionType in group.ExceptionTypes)
        {
            foreach (
                string indicator
                in ExceptionIndicators)
            {
                if (
                    exceptionType.Contains(
                        indicator,
                        StringComparison.OrdinalIgnoreCase))
                {
                    indicators.Add(exceptionType);
                }
            }
        }

        return indicators
            .OrderBy(
                value => value,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IncidentPriority DeterminePriority(
        LogGroupSummary group)
    {
        if (group.HighestLevel == LogLevel.Critical)
        {
            return IncidentPriority.Critical;
        }

        if (
            group.IsRecurring ||
            group.HighestLevel == LogLevel.Error)
        {
            return IncidentPriority.High;
        }

        return IncidentPriority.Medium;
    }

    private static double CalculateConfidence(
        LogGroupSummary group,
        int indicatorCount)
    {
        double confidence =
            76 +
            Math.Min(
                10,
                indicatorCount * 4);

        if (group.IsRecurring)
        {
            confidence += 7;
        }

        if (group.ExceptionTypes.Count > 0)
        {
            confidence += 5;
        }

        return Math.Min(
            100,
            confidence);
    }
}