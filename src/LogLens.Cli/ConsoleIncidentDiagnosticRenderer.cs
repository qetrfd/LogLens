using System.Globalization;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Cli;

internal static class ConsoleIncidentDiagnosticRenderer
{
    public static void Render(
        LogFileDiagnosticResult result,
        int top,
        IncidentPriority minimumPriority)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (top < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(top));
        }

        PrintSummary(result);
        PrintPriorityCounts(result.Diagnostics);

        IncidentDiagnosis[] visibleDiagnoses =
            result.Diagnoses
                .Where(
                    diagnosis =>
                        GetPriorityWeight(
                            diagnosis.Priority) >=
                        GetPriorityWeight(
                            minimumPriority))
                .Take(top)
                .ToArray();

        Console.WriteLine();
        Console.WriteLine("DIAGNÓSTICOS");
        Console.WriteLine(new string('─', 84));

        if (visibleDiagnoses.Length == 0)
        {
            Console.WriteLine(
                "No se detectaron diagnósticos con la prioridad solicitada.");

            return;
        }

        for (
            int index = 0;
            index < visibleDiagnoses.Length;
            index++)
        {
            PrintDiagnosis(
                visibleDiagnoses[index],
                index + 1);
        }

        int matchingCount =
            result.Diagnoses.Count(
                diagnosis =>
                    GetPriorityWeight(
                        diagnosis.Priority) >=
                    GetPriorityWeight(
                        minimumPriority));

        if (matchingCount > visibleDiagnoses.Length)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Se ocultaron " +
                $"{matchingCount - visibleDiagnoses.Length:N0} " +
                "diagnósticos adicionales.");
        }
    }

    private static void PrintSummary(
        LogFileDiagnosticResult result)
    {
        Console.WriteLine();
        Console.WriteLine("RESUMEN");
        Console.WriteLine(new string('─', 84));

        Console.WriteLine(
            $"Líneas totales:              {result.TotalLines,12:N0}");

        Console.WriteLine(
            $"Líneas procesadas:           {result.ParsedLines,12:N0}");

        Console.WriteLine(
            $"Líneas no reconocidas:       {result.UnparsedLines,12:N0}");

        Console.WriteLine(
            $"Grupos detectados:           {result.GroupCount,12:N0}");

        Console.WriteLine(
            $"Diagnósticos generados:      {result.DiagnosisCount,12:N0}");

        Console.WriteLine(
            $"Atención inmediata:          {result.ImmediateAttentionCount,12:N0}");

        Console.WriteLine(
            $"Incidentes críticos:         " +
            $"{(result.HasCriticalIncidents ? "Sí" : "No"),12}");
    }

    private static void PrintPriorityCounts(
        IncidentDiagnosticResult result)
    {
        Console.WriteLine();
        Console.WriteLine("PRIORIDADES");
        Console.WriteLine(new string('─', 84));

        IncidentPriority[] priorities =
        [
            IncidentPriority.Critical,
            IncidentPriority.High,
            IncidentPriority.Medium,
            IncidentPriority.Low,
            IncidentPriority.None
        ];

        foreach (IncidentPriority priority in priorities)
        {
            result.PriorityCounts.TryGetValue(
                priority,
                out int count);

            Console.WriteLine(
                $"{priority,-18} {count,12:N0}");
        }
    }

    private static void PrintDiagnosis(
        IncidentDiagnosis diagnosis,
        int position)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"#{position} · {diagnosis.Priority} · " +
            $"{diagnosis.ConfidencePercentage:0.##}% confianza");

        Console.WriteLine(new string('─', 84));

        Console.WriteLine(
            $"Regla:         {diagnosis.RuleId}");

        Console.WriteLine(
            $"Título:        {diagnosis.Title}");

        Console.WriteLine(
            $"Resumen:       {diagnosis.Summary}");

        Console.WriteLine(
            $"Huella:        {ShortFingerprint(diagnosis.Fingerprint)}");

        Console.WriteLine(
            $"Detectado:     {diagnosis.DetectedAt.ToString(
                "yyyy-MM-dd HH:mm:ss zzz",
                CultureInfo.InvariantCulture)}");

        Console.WriteLine("Evidencia:");

        foreach (
            DiagnosticEvidence evidence
            in diagnosis.Evidence)
        {
            Console.WriteLine(
                $"  - {evidence.Label}: {evidence.Value}");
        }

        Console.WriteLine("Acciones recomendadas:");

        for (
            int index = 0;
            index < diagnosis.RecommendedActions.Count;
            index++)
        {
            Console.WriteLine(
                $"  {index + 1}. " +
                $"{diagnosis.RecommendedActions[index]}");
        }
    }

    private static string ShortFingerprint(
        string value)
    {
        int length = Math.Min(
            value.Length,
            16);

        return value[..length];
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