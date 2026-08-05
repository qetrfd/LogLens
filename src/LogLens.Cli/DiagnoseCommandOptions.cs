using System.Globalization;
using LogLens.Core;

namespace LogLens.Cli;

internal sealed record DiagnoseCommandOptions
{
    public string FilePath { get; }

    public int SampleLimit { get; }

    public int Top { get; }

    public bool IncludeUnknownLevels { get; }

    public IncidentPriority MinimumPriority { get; }

    private DiagnoseCommandOptions(
        string filePath,
        int sampleLimit,
        int top,
        bool includeUnknownLevels,
        IncidentPriority minimumPriority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FilePath = filePath.Trim();
        SampleLimit = sampleLimit;
        Top = top;
        IncludeUnknownLevels = includeUnknownLevels;
        MinimumPriority = minimumPriority;
    }

    public static DiagnoseCommandOptions Parse(
        string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length < 2)
        {
            throw new ArgumentException(
                "Uso: loglens diagnose <archivo> " +
                "[--samples cantidad] " +
                "[--top cantidad] " +
                "[--exclude-unknown] " +
                "[--min-priority nivel]");
        }

        int sampleLimit = 3;
        int top = 20;
        bool includeUnknownLevels = true;

        IncidentPriority minimumPriority =
            IncidentPriority.Low;

        for (int index = 2; index < args.Length; index++)
        {
            string option = args[index]
                .Trim()
                .ToLowerInvariant();

            switch (option)
            {
                case "--samples":
                    sampleLimit = ReadNonNegativeInteger(
                        args,
                        ref index,
                        option);
                    break;

                case "--top":
                    top = ReadPositiveInteger(
                        args,
                        ref index,
                        option);
                    break;

                case "--exclude-unknown":
                    includeUnknownLevels = false;
                    break;

                case "--include-unknown":
                    includeUnknownLevels = true;
                    break;

                case "--min-priority":
                    minimumPriority = ReadPriority(
                        args,
                        ref index,
                        option);
                    break;

                default:
                    throw new ArgumentException(
                        $"Opción desconocida: {args[index]}");
            }
        }

        return new DiagnoseCommandOptions(
            args[1],
            sampleLimit,
            top,
            includeUnknownLevels,
            minimumPriority);
    }

    private static int ReadNonNegativeInteger(
        string[] args,
        ref int index,
        string option)
    {
        string value = ReadValue(
            args,
            ref index,
            option);

        if (
            !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int result) ||
            result < 0)
        {
            throw new ArgumentException(
                $"{option} debe ser un número no negativo.");
        }

        return result;
    }

    private static int ReadPositiveInteger(
        string[] args,
        ref int index,
        string option)
    {
        string value = ReadValue(
            args,
            ref index,
            option);

        if (
            !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int result) ||
            result < 1)
        {
            throw new ArgumentException(
                $"{option} debe ser un número mayor que cero.");
        }

        return result;
    }

    private static IncidentPriority ReadPriority(
        string[] args,
        ref int index,
        string option)
    {
        string value = ReadValue(
            args,
            ref index,
            option);

        return value.Trim().ToLowerInvariant() switch
        {
            "none" => IncidentPriority.None,
            "low" => IncidentPriority.Low,
            "medium" => IncidentPriority.Medium,
            "high" => IncidentPriority.High,
            "critical" => IncidentPriority.Critical,

            _ => throw new ArgumentException(
                "La prioridad debe ser: " +
                "none, low, medium, high o critical.")
        };
    }

    private static string ReadValue(
        string[] args,
        ref int index,
        string option)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException(
                $"Debes indicar un valor después de {option}.");
        }

        index++;

        return args[index];
    }
}