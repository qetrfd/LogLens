using System.Globalization;

namespace LogLens.Infrastructure;

public static class LogTimestampParser
{
    private static readonly string[] Formats =
    [
        "O",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
        "yyyy-MM-dd HH:mm:ss.FFFFFFFK",
        "yyyy-MM-dd HH:mm:ss.FFFFFFF",
        "yyyy/MM/dd HH:mm:ss.FFFFFFF",
        "dd/MM/yyyy HH:mm:ss.FFFFFFF",
        "MM/dd/yyyy HH:mm:ss.FFFFFFF"
    ];

    public static bool TryParse(
        string? value,
        out DateTimeOffset timestamp)
    {
        timestamp = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalized = value
            .Trim()
            .Trim('[', ']');

        if (
            DateTimeOffset.TryParseExact(
                normalized,
                Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces |
                DateTimeStyles.AssumeUniversal,
                out timestamp))
        {
            return true;
        }

        return DateTimeOffset.TryParse(
            normalized,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces |
            DateTimeStyles.AssumeUniversal,
            out timestamp);
    }

    public static bool TryParseUnix(
        long value,
        out DateTimeOffset timestamp)
    {
        timestamp = default;

        try
        {
            timestamp = Math.Abs(value) >= 100_000_000_000
                ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                : DateTimeOffset.FromUnixTimeSeconds(value);

            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}