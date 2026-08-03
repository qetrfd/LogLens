using System.Globalization;
using System.Text.RegularExpressions;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class GenericTextLogParser : ILogLineParser
{
    private const string LevelExpression =
        "TRACE|TRC|DEBUG|DBG|INFORMATION|INFO|INF|NOTICE|" +
        "WARNING|WARN|WRN|ERROR|ERR|FAILURE|FAILED|FAIL|" +
        "CRITICAL|CRIT|FATAL|PANIC";

    private static readonly Regex LinePattern = new(
        $$"""
        ^\s*
        (?:
            (?<timestamp>
                \d{4}[-/]\d{2}[-/]\d{2}
                [T\s]
                \d{2}:\d{2}:\d{2}
                (?:\.\d{1,7})?
                (?:Z|[+-]\d{2}:\d{2})?
            )
            \s+
        )?
        (?:
            \[(?<level>{{LevelExpression}})\]
            |
            (?<level>{{LevelExpression}})
        )?
        \s*
        (?:
            \[(?<service>[^\]\r\n]+)\]
            \s*
        )?
        (?<message>.*\S)?
        \s*$
        """,
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase |
        RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex StatusCodePattern = new(
        @"\b(?:http|status(?:code)?)\s*[:=]?\s*(?<value>[1-5]\d{2})\b",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase);

    private static readonly Regex BareErrorStatusPattern = new(
        @"\b(?<value>[45]\d{2})\b",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant);

    private static readonly Regex DurationPattern = new(
        @"\b(?<value>\d+)\s*(?:ms|milliseconds?)\b",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase);

    private static readonly Regex ExceptionPattern = new(
        @"\b(?<value>[A-Za-z_][A-Za-z0-9_.]*(?:Exception|Error))\b",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant);

    private static readonly Regex CorrelationPattern = new(
        """
        \b
        (?:
            correlation[-_\s]?id
            |
            request[-_\s]?id
            |
            trace[-_\s]?id
        )
        \s*[:=]\s*
        (?<value>[A-Za-z0-9._:/-]+)
        """,
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase |
        RegexOptions.IgnorePatternWhitespace);

    public string Name => "Generic text";

    public int Priority => 100;

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

        Match match = LinePattern.Match(line.Text);

        if (!match.Success)
        {
            return LogParseResult.NotMatched(
                Name,
                "La línea no coincide con el formato de texto.");
        }

        string message = match.Groups["message"].Value.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return LogParseResult.NotMatched(
                Name,
                "No se encontró un mensaje.");
        }

        DateTimeOffset? timestamp =
            ParseTimestamp(match.Groups["timestamp"].Value);

        LogLevel level = LogLevelParser.Parse(
            match.Groups["level"].Value);

        if (level == LogLevel.Unknown)
        {
            level = LogLevelParser.InferFromText(message);
        }

        string? service = NormalizeOptional(
            match.Groups["service"].Value);

        int? statusCode = ParseStatusCode(message);
        long? duration = ParseDuration(message);

        string? exceptionType = ParseValue(
            ExceptionPattern,
            message);

        string? correlationId = ParseValue(
            CorrelationPattern,
            message);

        Dictionary<string, string> metadata =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["parser"] = Name,
                ["sourceName"] = context.SourceName,
                ["filePath"] = context.FilePath
            };

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

    private static DateTimeOffset? ParseTimestamp(
        string value)
    {
        return LogTimestampParser.TryParse(
            value,
            out DateTimeOffset timestamp)
            ? timestamp
            : null;
    }

    private static int? ParseStatusCode(string message)
    {
        Match match = StatusCodePattern.Match(message);

        if (!match.Success)
        {
            match = BareErrorStatusPattern.Match(message);
        }

        if (
            match.Success &&
            int.TryParse(
                match.Groups["value"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int statusCode))
        {
            return statusCode;
        }

        return null;
    }

    private static long? ParseDuration(string message)
    {
        Match match = DurationPattern.Match(message);

        if (
            match.Success &&
            long.TryParse(
                match.Groups["value"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out long duration))
        {
            return duration;
        }

        return null;
    }

    private static string? ParseValue(
        Regex pattern,
        string message)
    {
        Match match = pattern.Match(message);

        return match.Success
            ? NormalizeOptional(
                match.Groups["value"].Value)
            : null;
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}