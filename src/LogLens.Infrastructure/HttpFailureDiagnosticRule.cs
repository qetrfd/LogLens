using System.Globalization;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class HttpFailureDiagnosticRule
    : IIncidentDiagnosticRule
{
    public string Id =>
        "http-failure";

    public string Name =>
        "HTTP failure";

    public int Order =>
        300;

    public IncidentDiagnosis? Evaluate(
        IncidentDiagnosticContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        LogGroupSummary group =
            context.Group;

        if (group.StatusCodes.Count == 0)
        {
            return null;
        }

        bool hasServerError =
            group.StatusCodes.Any(
                code => code >= 500);

        bool hasRateLimit =
            group.StatusCodes.Contains(429);

        bool hasClientError =
            group.StatusCodes.Any(
                code => code is >= 400 and < 500);

        if (
            !hasServerError &&
            !hasClientError)
        {
            return null;
        }

        IncidentPriority priority =
            DeterminePriority(
                group,
                hasServerError,
                hasRateLimit);

        double confidence =
            CalculateConfidence(
                group,
                hasServerError,
                hasRateLimit);

        List<DiagnosticEvidence> evidence =
        [
            new(
                "http-status-codes",
                "Códigos HTTP",
                string.Join(
                    ", ",
                    group.StatusCodes)),

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
                "group-percentage",
                "Porcentaje de entradas",
                $"{context.GroupPercentage:0.##}%"),

            new(
                "representative-message",
                "Mensaje representativo",
                group.RepresentativeMessage)
        ];

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

        string title;
        string summary;
        string[] actions;

        if (hasServerError)
        {
            title =
                "Fallo HTTP del servidor";

            summary =
                "Se detectaron respuestas HTTP 5xx que pueden indicar " +
                "un error interno, una dependencia caída o una " +
                "configuración incorrecta.";

            actions =
            [
                "Revisar la excepción y el stack trace asociados a las respuestas 5xx.",
                "Verificar la disponibilidad de bases de datos, APIs y servicios dependientes.",
                "Identificar los endpoints o procesos que generaron las respuestas.",
                "Revisar despliegues y cambios de configuración recientes."
            ];
        }
        else if (hasRateLimit)
        {
            title =
                "Límite de solicitudes alcanzado";

            summary =
                "Se detectaron respuestas HTTP 429 relacionadas con " +
                "límites de solicitudes o saturación del servicio.";

            actions =
            [
                "Revisar los límites configurados para el servicio o proveedor externo.",
                "Aplicar reintentos con espera exponencial y dispersión aleatoria.",
                "Reducir solicitudes duplicadas o innecesarias.",
                "Confirmar si existen picos anormales de tráfico."
            ];
        }
        else
        {
            title =
                "Errores HTTP del cliente";

            summary =
                "Se detectaron respuestas HTTP 4xx relacionadas con " +
                "solicitudes inválidas, autenticación, permisos o recursos ausentes.";

            actions =
            [
                "Revisar los parámetros, encabezados y cuerpo de las solicitudes.",
                "Confirmar la autenticación y los permisos del cliente.",
                "Verificar las rutas y recursos solicitados.",
                "Comparar los errores con cambios recientes del contrato de la API."
            ];
        }

        return new IncidentDiagnosis(
            Id,
            title,
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
        bool hasServerError,
        bool hasRateLimit)
    {
        if (
            group.HighestLevel == LogLevel.Critical ||
            hasServerError &&
            group.OccurrenceCount >= 10)
        {
            return IncidentPriority.Critical;
        }

        if (
            hasServerError ||
            hasRateLimit &&
            group.IsRecurring)
        {
            return IncidentPriority.High;
        }

        if (group.IsRecurring)
        {
            return IncidentPriority.Medium;
        }

        return IncidentPriority.Low;
    }

    private static double CalculateConfidence(
        LogGroupSummary group,
        bool hasServerError,
        bool hasRateLimit)
    {
        double confidence = 76;

        if (hasServerError)
        {
            confidence += 10;
        }

        if (hasRateLimit)
        {
            confidence += 6;
        }

        if (group.IsRecurring)
        {
            confidence += 5;
        }

        confidence += Math.Min(
            3,
            group.StatusCodes.Count);

        return Math.Min(
            100,
            confidence);
    }
}