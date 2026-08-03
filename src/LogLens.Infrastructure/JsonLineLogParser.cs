using System.Globalization;
using System.Text.Json;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class JsonLineLogParser : ILogLineParser
{
    private static readonly string[] TimestampNames =
    [
        "@timestamp",
        "timestamp",
        "time",
        "datetime",
        "date",
        "createdAt"
    ];

    private static readonly string[] LevelNames =
    [
        "level",
        "severity",
        "logLevel",
        "levelName"
    ];

    private static readonly string[] MessageNames =
    [
        "message",
        "msg",
        "text",
        "description",
        "event"
    ];

    private static readonly string[] ServiceNames =
    [
        "service",
        "serviceName",
        "application",
        "app",
        "component",
        "source"
    ];

    private static readonly string[] ExceptionNames =
    [
        "exception",
        "exceptionType",
        "errorType",
        "error"
    ];

    private static readonly string[] StatusCodeNames =
    [
        "statusCode",
        "status",
        "httpStatus",
        "responseCode"
    ];

    private static readonly string[] DurationNames =
    [
        "durationMs",
        "durationMilliseconds",
        "elapsedMs",
        "latencyMs"
    ];

    private static readonly string[] CorrelationNames =
    [
        "correlationId",
        "requestId",
        "traceId",
        "spanId"
    ];

    public string Name => "JSON lines";

    public int Priority => 300;

    public LogParseResult Parse(
        RawLogLine line,
        LogParserContext context)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(context);

        if (line.IsEmpty)
        {
            return LogParseResult.NotMatched(
                Name,
                "La línea está vacía.");
        }

        string trimmed = line.Text.Trim();

        if (
            !trimmed.StartsWith(
                "{",
                StringComparison.Ordinal) ||
            !trimmed.EndsWith(
                "}",
                StringComparison.Ordinal))
        {
            return LogParseResult.NotMatched(
                Name,
                "La línea no parece ser un objeto JSON.");
        }

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(trimmed);

            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return LogParseResult.NotMatched(
                    Name,
                    "El valor JSON no es un objeto.");
            }

            string? message = ReadString(
                root,
                MessageNames);

            if (string.IsNullOrWhiteSpace(message))
            {
                return LogParseResult.NotMatched(
                    Name,
                    "El objeto JSON no contiene un mensaje.");
            }

            DateTimeOffset? timestamp =
                ReadTimestamp(root);

            LogLevel level = ReadLevel(root);

            if (level == LogLevel.Unknown)
            {
                level = LogLevelParser.InferFromText(message);
            }

            string? service = ReadString(
                root,
                ServiceNames);

            string? exceptionType = ReadString(
                root,
                ExceptionNames);

            int? statusCode = ReadInteger(
                root,
                StatusCodeNames);

            long? duration = ReadLong(
                root,
                DurationNames);

            string? correlationId = ReadString(
                root,
                CorrelationNames);

            Dictionary<string, string> metadata =
                ReadMetadata(root);

            metadata["parser"] = Name;
            metadata["sourceName"] = context.SourceName;
            metadata["filePath"] = context.FilePath;

            ParsedLogLine parsedLine = new(
                line.SourceId,
                line.LineNumber,
                timestamp,
                level,
                message,
                line.Text,
                service,
                exceptionType,
                statusCode,
                duration,
                correlationId,
                metadata);

            return LogParseResult.Parsed(
                Name,
                parsedLine);
        }
        catch (JsonException exception)
        {
            return LogParseResult.NotMatched(
                Name,
                exception.Message);
        }
    }

    private static DateTimeOffset? ReadTimestamp(
        JsonElement root)
    {
        if (
            !TryGetProperty(
                root,
                TimestampNames,
                out JsonElement value))
        {
            return null;
        }

        if (
            value.ValueKind == JsonValueKind.String &&
            LogTimestampParser.TryParse(
                value.GetString(),
                out DateTimeOffset timestamp))
        {
            return timestamp;
        }

        if (
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt64(out long unixValue) &&
            LogTimestampParser.TryParseUnix(
                unixValue,
                out timestamp))
        {
            return timestamp;
        }

        return null;
    }

    private static LogLevel ReadLevel(
        JsonElement root)
    {
        if (
            !TryGetProperty(
                root,
                LevelNames,
                out JsonElement value))
        {
            return LogLevel.Unknown;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return LogLevelParser.Parse(
                value.GetString());
        }

        if (
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out int numericLevel) &&
            Enum.IsDefined(
                typeof(LogLevel),
                numericLevel))
        {
            return (LogLevel)numericLevel;
        }

        return LogLevel.Unknown;
    }

    private static string? ReadString(
        JsonElement root,
        IReadOnlyList<string> names)
    {
        if (
            !TryGetProperty(
                root,
                names,
                out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String =>
                NormalizeOptional(value.GetString()),

            JsonValueKind.Number =>
                value.GetRawText(),

            JsonValueKind.True =>
                bool.TrueString,

            JsonValueKind.False =>
                bool.FalseString,

            _ => null
        };
    }

    private static int? ReadInteger(
        JsonElement root,
        IReadOnlyList<string> names)
    {
        if (
            !TryGetProperty(
                root,
                names,
                out JsonElement value))
        {
            return null;
        }

        if (
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out int numericValue))
        {
            return numericValue;
        }

        if (
            value.ValueKind == JsonValueKind.String &&
            int.TryParse(
                value.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out numericValue))
        {
            return numericValue;
        }

        return null;
    }

    private static long? ReadLong(
        JsonElement root,
        IReadOnlyList<string> names)
    {
        if (
            !TryGetProperty(
                root,
                names,
                out JsonElement value))
        {
            return null;
        }

        if (
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt64(out long numericValue))
        {
            return numericValue;
        }

        if (
            value.ValueKind == JsonValueKind.String &&
            long.TryParse(
                value.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out numericValue))
        {
            return numericValue;
        }

        return null;
    }

    private static Dictionary<string, string> ReadMetadata(
        JsonElement root)
    {
        Dictionary<string, string> metadata =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (JsonProperty property in root.EnumerateObject())
        {
            string? value = property.Value.ValueKind switch
            {
                JsonValueKind.String =>
                    property.Value.GetString(),

                JsonValueKind.Number =>
                    property.Value.GetRawText(),

                JsonValueKind.True =>
                    bool.TrueString,

                JsonValueKind.False =>
                    bool.FalseString,

                JsonValueKind.Null =>
                    null,

                _ => property.Value.GetRawText()
            };

            if (value is not null)
            {
                metadata[property.Name] = value;
            }
        }

        return metadata;
    }

    private static bool TryGetProperty(
        JsonElement root,
        IReadOnlyList<string> names,
        out JsonElement value)
    {
        foreach (JsonProperty property in root.EnumerateObject())
        {
            foreach (string name in names)
            {
                if (
                    string.Equals(
                        property.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;

                    return true;
                }
            }
        }

        value = default;

        return false;
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}