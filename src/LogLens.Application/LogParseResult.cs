using LogLens.Core;

namespace LogLens.Application;

public sealed record LogParseResult
{
    public bool Success { get; }

    public string ParserName { get; }

    public ParsedLogLine? ParsedLine { get; }

    public string? FailureReason { get; }

    private LogParseResult(
        bool success,
        string parserName,
        ParsedLogLine? parsedLine,
        string? failureReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parserName);

        if (success && parsedLine is null)
        {
            throw new ArgumentException(
                "Un resultado correcto debe contener una línea procesada.",
                nameof(parsedLine));
        }

        if (!success && parsedLine is not null)
        {
            throw new ArgumentException(
                "Un resultado fallido no puede contener una línea procesada.",
                nameof(parsedLine));
        }

        Success = success;
        ParserName = parserName.Trim();
        ParsedLine = parsedLine;
        FailureReason = NormalizeOptional(failureReason);
    }

    public static LogParseResult Parsed(
        string parserName,
        ParsedLogLine parsedLine)
    {
        ArgumentNullException.ThrowIfNull(parsedLine);

        return new LogParseResult(
            true,
            parserName,
            parsedLine,
            null);
    }

    public static LogParseResult NotMatched(
        string parserName,
        string? reason = null)
    {
        return new LogParseResult(
            false,
            parserName,
            null,
            reason);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}