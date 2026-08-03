using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class DefaultLogFingerprintGenerator
    : ILogFingerprintGenerator
{
    private static readonly Regex TimestampPattern = new(
    @"(?<!\d)\d{4}[-/]\d{2}[-/]\d{2}[T\s]\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?(?!\d)",
    RegexOptions.Compiled |
    RegexOptions.CultureInvariant |
    RegexOptions.IgnoreCase);
    private static readonly Regex UrlPattern = new(
        @"\bhttps?://[^\s]+",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase);

    private static readonly Regex GuidPattern = new(
        """
        \b
        [0-9a-f]{8}
        -
        [0-9a-f]{4}
        -
        [0-9a-f]{4}
        -
        [0-9a-f]{4}
        -
        [0-9a-f]{12}
        \b
        """,
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase |
        RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex IpAddressPattern = new(
        """
        \b
        (?:
            \d{1,3}\.
        ){3}
        \d{1,3}
        (?::\d{1,5})?
        \b
        """,
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex IdentifierPattern = new(
        """
        (?<key>
            \b
            (?:
                request
                |
                correlation
                |
                trace
                |
                span
            )
            [-_\s]?
            id
        )
        \s*[:=]\s*
        [A-Za-z0-9._:/-]+
        """,
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase |
        RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex HexadecimalPattern = new(
        @"\b0x[0-9a-f]+\b",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnoreCase);

    private static readonly Regex NumberPattern = new(
        @"(?<![A-Za-z])[-+]?\d+(?:\.\d+)?(?![A-Za-z])",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant);

    private static readonly Regex WhitespacePattern = new(
        @"\s+",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant);

    public LogFingerprint Generate(
        ParsedLogLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        string normalizedMessage =
            NormalizeMessage(line.Message);

        string seed = string.Join(
            "|",
            line.Level.ToString(),
            NormalizeToken(line.Service),
            NormalizeToken(line.ExceptionType),
            line.StatusCode?.ToString(
                CultureInfo.InvariantCulture)
                ?? string.Empty,
            normalizedMessage);

        byte[] sourceBytes =
            Encoding.UTF8.GetBytes(seed);

        byte[] hash =
            SHA256.HashData(sourceBytes);

        string fingerprint =
            Convert.ToHexString(hash)
                .ToLowerInvariant();

        return new LogFingerprint(
            fingerprint,
            normalizedMessage);
    }

    private static string NormalizeMessage(
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        string normalized =
            message.Trim().ToLowerInvariant();

        normalized = TimestampPattern.Replace(
            normalized,
            "<timestamp>");

        normalized = UrlPattern.Replace(
            normalized,
            "<url>");

        normalized = GuidPattern.Replace(
            normalized,
            "<guid>");

        normalized = IpAddressPattern.Replace(
            normalized,
            "<ip>");

        normalized = IdentifierPattern.Replace(
            normalized,
            "${key}=<id>");

        normalized = HexadecimalPattern.Replace(
            normalized,
            "<hex>");

        normalized = NumberPattern.Replace(
            normalized,
            "<number>");

        normalized = WhitespacePattern.Replace(
            normalized,
            " ");

        return normalized.Trim();
    }

    private static string NormalizeToken(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }
}