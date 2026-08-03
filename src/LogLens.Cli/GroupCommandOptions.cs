using System.Globalization;

namespace LogLens.Cli;

internal sealed record GroupCommandOptions
{
    public string FilePath { get; }

    public int SampleLimit { get; }

    public int Top { get; }

    public bool IncludeUnknownLevels { get; }

    private GroupCommandOptions(
        string filePath,
        int sampleLimit,
        int top,
        bool includeUnknownLevels)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FilePath = filePath.Trim();
        SampleLimit = sampleLimit;
        Top = top;
        IncludeUnknownLevels = includeUnknownLevels;
    }

    public static GroupCommandOptions Parse(
        string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length < 2)
        {
            throw new ArgumentException(
                "Uso: loglens group <archivo> " +
                "[--samples cantidad] " +
                "[--top cantidad] " +
                "[--exclude-unknown]");
        }

        int sampleLimit = 3;
        int top = 20;
        bool includeUnknownLevels = true;

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
                        "--samples");
                    break;

                case "--top":
                    top = ReadPositiveInteger(
                        args,
                        ref index,
                        "--top");
                    break;

                case "--exclude-unknown":
                    includeUnknownLevels = false;
                    break;

                case "--include-unknown":
                    includeUnknownLevels = true;
                    break;

                default:
                    throw new ArgumentException(
                        $"Opción desconocida: {args[index]}");
            }
        }

        return new GroupCommandOptions(
            args[1],
            sampleLimit,
            top,
            includeUnknownLevels);
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