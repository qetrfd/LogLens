namespace LogLens.Core;

public static class SupportedLogFileExtensions
{
    private static readonly string[] Values =
    [
        ".jsonl",
        ".log",
        ".ndjson",
        ".txt"
    ];

    private static readonly HashSet<string> Lookup =
        new(Values, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> All => Values;

    public static bool IsSupported(string? pathOrExtension)
    {
        if (string.IsNullOrWhiteSpace(pathOrExtension))
        {
            return false;
        }

        string candidate = pathOrExtension.Trim();
        string extension = Path.GetExtension(candidate);

        if (
            string.IsNullOrWhiteSpace(extension) &&
            candidate.StartsWith(".", StringComparison.Ordinal))
        {
            extension = candidate;
        }

        return Lookup.Contains(extension);
    }
}
