using System.Globalization;
using System.Text;
using LogLens.Application;
using LogLens.Core;
using LogLens.Infrastructure;

namespace LogLens.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        using CancellationTokenSource cancellationSource = new();

        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
        };

        Console.CancelKeyPress += cancelHandler;

        try
        {
            return await RunAsync(
                args,
                cancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            Console.Error.WriteLine(
                "La operación fue cancelada.");

            return 130;
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.Error.WriteLine(
                $"Error: {exception.Message}");

            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }

    private static async Task<int> RunAsync(
        string[] args,
        CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            PrintStartupSummary();
            PrintHelp();

            return 0;
        }

        string command = args[0]
            .Trim()
            .ToLowerInvariant();

        return command switch
        {
            "read" => await RunReadAsync(
                args,
                cancellationToken),

            "parse" => await RunParseAsync(
                args,
                cancellationToken),

            "version" => PrintVersion(),
            "--version" => PrintVersion(),
            "-v" => PrintVersion(),

            "help" => PrintHelpAndReturn(),
            "--help" => PrintHelpAndReturn(),
            "-h" => PrintHelpAndReturn(),

            _ => PrintUnknownCommand(command)
        };
    }

    private static async Task<int> RunReadAsync(
        string[] args,
        CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(
            args,
            "loglens read <archivo> [--preview cantidad]");

        int previewLimit = ParsePreviewLimit(args);
        Guid sourceId = Guid.NewGuid();

        LogReadRequest request = new(
            sourceId,
            filePath,
            progressIntervalLines: 250);

        LogFileInspectionService service = new(
            new StreamingLogFileReader());

        ConsoleLogProgress progress = new();

        Console.WriteLine();
        Console.WriteLine(
            "LOGLENS · LECTOR PROGRESIVO");

        Console.WriteLine(
            new string('─', 68));

        Console.WriteLine(
            $"Archivo: {request.FilePath}");

        Console.WriteLine();

        LogFileInspectionResult result =
            await service.InspectAsync(
                request,
                previewLimit,
                progress,
                cancellationToken);

        FileInfo fileInfo = new(result.FilePath);

        Console.WriteLine();
        Console.WriteLine("RESUMEN");
        Console.WriteLine(new string('─', 68));

        Console.WriteLine(
            $"Tamaño:              {FormatBytes(fileInfo.Length)}");

        Console.WriteLine(
            $"Líneas:              {result.TotalLines:N0}");

        Console.WriteLine(
            $"Líneas vacías:       {result.EmptyLines:N0}");

        Console.WriteLine(
            $"Línea más larga:     {result.LongestLineLength:N0} caracteres");

        Console.WriteLine(
            $"Vista previa:        {result.Preview.Count:N0} líneas");

        if (result.Preview.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("VISTA PREVIA");
            Console.WriteLine(new string('─', 68));

            foreach (RawLogLine line in result.Preview)
            {
                Console.WriteLine(
                    $"{line.LineNumber,6} │ {line.Text}");
            }
        }

        Console.WriteLine();

        return 0;
    }

    private static async Task<int> RunParseAsync(
        string[] args,
        CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(
            args,
            "loglens parse <archivo> [--preview cantidad]");

        int previewLimit = ParsePreviewLimit(args);
        Guid sourceId = Guid.NewGuid();

        LogReadRequest request = new(
            sourceId,
            filePath,
            progressIntervalLines: 250);

        string sourceName =
            Path.GetFileNameWithoutExtension(
                request.FilePath);

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            sourceName = "Archivo";
        }

        LogFileParsingService service = new(
            new StreamingLogFileReader(),
            DefaultLogParserFactory.Create());

        ConsoleLogProgress progress = new();

        Console.WriteLine();
        Console.WriteLine(
            "LOGLENS · ANÁLISIS DE ESTRUCTURA");

        Console.WriteLine(
            new string('─', 68));

        Console.WriteLine(
            $"Archivo: {request.FilePath}");

        Console.WriteLine();

        LogFileParsingResult result =
            await service.ParseAsync(
                request,
                sourceName,
                previewLimit,
                progress,
                cancellationToken);

        Console.WriteLine();
        Console.WriteLine("RESUMEN");
        Console.WriteLine(new string('─', 68));

        Console.WriteLine(
            $"Líneas totales:      {result.TotalLines:N0}");

        Console.WriteLine(
            $"Procesadas:          {result.ParsedLines:N0}");

        Console.WriteLine(
            $"No reconocidas:      {result.UnparsedLines:N0}");

        Console.WriteLine(
            $"Cobertura:           {result.ParsedPercentage:0.##}%");

        PrintLevelCounts(result);
        PrintParserCounts(result);
        PrintParsedPreview(result);

        Console.WriteLine();

        return result.UnparsedLines > 0
            ? 2
            : 0;
    }

    private static void PrintLevelCounts(
        LogFileParsingResult result)
    {
        Console.WriteLine();
        Console.WriteLine("NIVELES");
        Console.WriteLine(new string('─', 68));

        if (result.LevelCounts.Count == 0)
        {
            Console.WriteLine(
                "No se detectaron niveles.");

            return;
        }

        foreach (
            KeyValuePair<LogLevel, long> item
            in result.LevelCounts
                .OrderByDescending(item => item.Key))
        {
            Console.WriteLine(
                $"{item.Key,-18} {item.Value,10:N0}");
        }
    }

    private static void PrintParserCounts(
        LogFileParsingResult result)
    {
        Console.WriteLine();
        Console.WriteLine("PARSERS");
        Console.WriteLine(new string('─', 68));

        if (result.ParserCounts.Count == 0)
        {
            Console.WriteLine(
                "Ningún parser reconoció entradas.");

            return;
        }

        foreach (
            KeyValuePair<string, long> item
            in result.ParserCounts
                .OrderByDescending(item => item.Value)
                .ThenBy(
                    item => item.Key,
                    StringComparer.Ordinal))
        {
            Console.WriteLine(
                $"{item.Key,-30} {item.Value,10:N0}");
        }
    }

    private static void PrintParsedPreview(
        LogFileParsingResult result)
    {
        if (result.Preview.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("VISTA PREVIA PROCESADA");
        Console.WriteLine(new string('─', 68));

        foreach (ParsedLogLine line in result.Preview)
        {
            string timestamp = line.Timestamp?.ToString(
                "yyyy-MM-dd HH:mm:ss zzz",
                CultureInfo.InvariantCulture)
                ?? "sin fecha";

            string service = line.Service is null
                ? string.Empty
                : $" [{line.Service}]";

            Console.WriteLine(
                $"{line.LineNumber,6} │ " +
                $"{timestamp} │ " +
                $"{line.Level,-11}{service} │ " +
                $"{line.Message}");
        }
    }

    private static string GetFilePath(
        string[] args,
        string usage)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException(
                $"Uso: {usage}");
        }

        return args[1];
    }

    private static int ParsePreviewLimit(
        string[] args)
    {
        int previewLimit = 10;

        for (int index = 2; index < args.Length; index++)
        {
            if (
                !string.Equals(
                    args[index],
                    "--preview",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Opción desconocida: {args[index]}");
            }

            if (index + 1 >= args.Length)
            {
                throw new ArgumentException(
                    "Debes indicar una cantidad después de --preview.");
            }

            if (
                !int.TryParse(
                    args[index + 1],
                    out previewLimit) ||
                previewLimit < 0)
            {
                throw new ArgumentException(
                    "La cantidad de vista previa debe ser un número no negativo.");
            }

            index++;
        }

        return previewLimit;
    }

    private static int PrintVersion()
    {
        Console.WriteLine(
            LogLensProduct.Current.Version);

        return 0;
    }

    private static void PrintStartupSummary()
    {
        StartupSummaryService startupService = new(
            new RuntimeEnvironmentProvider());

        StartupSummary summary =
            startupService.Create();

        Console.WriteLine();
        Console.WriteLine("LOGLENS");
        Console.WriteLine(new string('─', 68));

        Console.WriteLine(
            $"Versión:       {summary.Product.Version}");

        Console.WriteLine(
            $"Descripción:   {summary.Product.Description}");

        Console.WriteLine(
            $"Sistema:       {summary.Runtime.OperatingSystem}");

        Console.WriteLine(
            $"Arquitectura:  {summary.Runtime.Architecture}");

        Console.WriteLine(
            $"Runtime:       {summary.Runtime.Framework}");

        Console.WriteLine(new string('─', 68));
        Console.WriteLine();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("COMANDOS");
        Console.WriteLine();

        Console.WriteLine(
            "  loglens read <archivo>");

        Console.WriteLine(
            "  loglens read <archivo> --preview 20");

        Console.WriteLine(
            "  loglens parse <archivo>");

        Console.WriteLine(
            "  loglens parse <archivo> --preview 20");

        Console.WriteLine(
            "  loglens version");

        Console.WriteLine(
            "  loglens help");

        Console.WriteLine();
        Console.WriteLine("FORMATOS");
        Console.WriteLine();

        Console.WriteLine(
            $"  {string.Join(", ", SupportedLogFileExtensions.All)}");

        Console.WriteLine();
    }

    private static int PrintHelpAndReturn()
    {
        PrintHelp();

        return 0;
    }

    private static int PrintUnknownCommand(
        string command)
    {
        Console.Error.WriteLine(
            $"Comando desconocido: {command}");

        Console.Error.WriteLine();

        PrintHelp();

        return 1;
    }

    private static string FormatBytes(
        long bytes)
    {
        string[] units =
        [
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        ];

        double size = bytes;
        int unitIndex = 0;

        while (
            size >= 1024 &&
            unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}