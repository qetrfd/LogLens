using System.Globalization;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Cli;

internal static class ConsoleLogGroupRenderer
{
    public static void Render(
        LogFileGroupingResult result,
        int top)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (top < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(top));
        }

        PrintSummary(result);

        LogGroupSummary[] visibleGroups =
            result.Groups
                .Take(top)
                .ToArray();

        Console.WriteLine();
        Console.WriteLine("GRUPOS DETECTADOS");
        Console.WriteLine(new string('─', 78));

        if (visibleGroups.Length == 0)
        {
            Console.WriteLine(
                "No se detectaron grupos.");

            return;
        }

        for (
            int index = 0;
            index < visibleGroups.Length;
            index++)
        {
            PrintGroup(
                visibleGroups[index],
                index + 1);
        }

        if (result.GroupCount > visibleGroups.Length)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Se ocultaron " +
                $"{result.GroupCount - visibleGroups.Length:N0} " +
                "grupos adicionales.");
        }
    }

    private static void PrintSummary(
        LogFileGroupingResult result)
    {
        Console.WriteLine();
        Console.WriteLine("RESUMEN");
        Console.WriteLine(new string('─', 78));

        Console.WriteLine(
            $"Líneas totales:          {result.TotalLines,12:N0}");

        Console.WriteLine(
            $"Líneas procesadas:       {result.ParsedLines,12:N0}");

        Console.WriteLine(
            $"Líneas no reconocidas:   {result.UnparsedLines,12:N0}");

        Console.WriteLine(
            $"Cobertura:               {result.ParsedPercentage,11:0.##}%");

        Console.WriteLine(
            $"Entradas agrupadas:      {result.Grouping.GroupedEntries,12:N0}");

        Console.WriteLine(
            $"Entradas excluidas:      {result.Grouping.ExcludedEntries,12:N0}");

        Console.WriteLine(
            $"Grupos detectados:       {result.GroupCount,12:N0}");

        Console.WriteLine(
            $"Grupos recurrentes:      {result.RecurringGroupCount,12:N0}");

        Console.WriteLine(
            $"Entradas recurrentes:    " +
            $"{result.RecurringEntryPercentage,11:0.##}%");
    }

    private static void PrintGroup(
        LogGroupSummary group,
        int position)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"#{position} · " +
            $"{group.OccurrenceCount:N0} apariciones · " +
            $"{group.HighestLevel}");

        Console.WriteLine(
            new string('─', 78));

        Console.WriteLine(
            $"Mensaje:       {group.RepresentativeMessage}");

        Console.WriteLine(
            $"Normalizado:   {group.Fingerprint.NormalizedMessage}");

        Console.WriteLine(
            $"Huella:        {ShortFingerprint(group.Fingerprint.Value)}");

        if (group.Services.Count > 0)
        {
            Console.WriteLine(
                $"Servicios:     {string.Join(", ", group.Services)}");
        }

        if (group.ExceptionTypes.Count > 0)
        {
            Console.WriteLine(
                $"Excepciones:   " +
                $"{string.Join(", ", group.ExceptionTypes)}");
        }

        if (group.StatusCodes.Count > 0)
        {
            Console.WriteLine(
                $"Códigos HTTP:  " +
                $"{string.Join(", ", group.StatusCodes)}");
        }

        if (
            group.FirstSeen.HasValue ||
            group.LastSeen.HasValue)
        {
            Console.WriteLine(
                $"Primera vez:   {FormatTimestamp(group.FirstSeen)}");

            Console.WriteLine(
                $"Última vez:    {FormatTimestamp(group.LastSeen)}");
        }

        if (group.Samples.Count > 0)
        {
            Console.WriteLine("Muestras:");

            foreach (LogGroupSample sample in group.Samples)
            {
                Console.WriteLine(
                    $"  Línea {sample.LineNumber,6:N0} │ " +
                    $"{FormatTimestamp(sample.Timestamp)} │ " +
                    $"{sample.Message}");
            }
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

    private static string FormatTimestamp(
        DateTimeOffset? timestamp)
    {
        return timestamp?.ToString(
            "yyyy-MM-dd HH:mm:ss zzz",
            CultureInfo.InvariantCulture)
            ?? "sin fecha";
    }
}