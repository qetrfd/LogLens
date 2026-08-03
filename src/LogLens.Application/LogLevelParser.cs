using LogLens.Core;

namespace LogLens.Infrastructure;

public static class LogLevelParser
{
    private static readonly Dictionary<string, LogLevel> Levels =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["trace"] = LogLevel.Trace,
            ["trc"] = LogLevel.Trace,
            ["verbose"] = LogLevel.Trace,

            ["debug"] = LogLevel.Debug,
            ["dbg"] = LogLevel.Debug,

            ["information"] = LogLevel.Information,
            ["info"] = LogLevel.Information,
            ["inf"] = LogLevel.Information,
            ["notice"] = LogLevel.Information,

            ["warning"] = LogLevel.Warning,
            ["warn"] = LogLevel.Warning,
            ["wrn"] = LogLevel.Warning,

            ["error"] = LogLevel.Error,
            ["err"] = LogLevel.Error,
            ["failure"] = LogLevel.Error,
            ["failed"] = LogLevel.Error,
            ["fail"] = LogLevel.Error,

            ["critical"] = LogLevel.Critical,
            ["crit"] = LogLevel.Critical,
            ["fatal"] = LogLevel.Critical,
            ["panic"] = LogLevel.Critical
        };

    public static LogLevel Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return LogLevel.Unknown;
        }

        string normalized = value
            .Trim()
            .Trim('[', ']', '(', ')', ':')
            .Trim();

        return Levels.TryGetValue(
            normalized,
            out LogLevel level)
            ? level
            : LogLevel.Unknown;
    }

    public static bool TryParse(
        string? value,
        out LogLevel level)
    {
        level = Parse(value);

        return level != LogLevel.Unknown;
    }

    public static LogLevel InferFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return LogLevel.Unknown;
        }

        string normalized = text.ToLowerInvariant();

        if (
            normalized.Contains("critical", StringComparison.Ordinal) ||
            normalized.Contains("fatal", StringComparison.Ordinal) ||
            normalized.Contains("panic", StringComparison.Ordinal))
        {
            return LogLevel.Critical;
        }

        if (
            normalized.Contains("error", StringComparison.Ordinal) ||
            normalized.Contains("exception", StringComparison.Ordinal) ||
            normalized.Contains("failed", StringComparison.Ordinal) ||
            normalized.Contains("failure", StringComparison.Ordinal))
        {
            return LogLevel.Error;
        }

        if (
            normalized.Contains("warning", StringComparison.Ordinal) ||
            normalized.Contains("warn", StringComparison.Ordinal))
        {
            return LogLevel.Warning;
        }

        if (
            normalized.Contains("debug", StringComparison.Ordinal))
        {
            return LogLevel.Debug;
        }

        if (
            normalized.Contains("trace", StringComparison.Ordinal))
        {
            return LogLevel.Trace;
        }

        if (
            normalized.Contains("information", StringComparison.Ordinal) ||
            normalized.Contains("info", StringComparison.Ordinal))
        {
            return LogLevel.Information;
        }

        return LogLevel.Unknown;
    }
}