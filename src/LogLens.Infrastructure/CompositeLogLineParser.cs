using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class CompositeLogLineParser : ILogLineParser
{
    private readonly IReadOnlyList<ILogLineParser> _parsers;

    public string Name => "Composite";

    public int Priority => int.MaxValue;

    public CompositeLogLineParser(
        IEnumerable<ILogLineParser> parsers)
    {
        ArgumentNullException.ThrowIfNull(parsers);

        ILogLineParser[] normalizedParsers = parsers
            .Where(parser => parser is not null)
            .Where(parser => parser is not CompositeLogLineParser)
            .OrderByDescending(parser => parser.Priority)
            .ThenBy(parser => parser.Name, StringComparer.Ordinal)
            .ToArray();

        if (normalizedParsers.Length == 0)
        {
            throw new ArgumentException(
                "Debe proporcionarse al menos un parser.",
                nameof(parsers));
        }

        _parsers = normalizedParsers;
    }

    public LogParseResult Parse(
        RawLogLine line,
        LogParserContext context)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(context);

        List<string> failures = [];

        foreach (ILogLineParser parser in _parsers)
        {
            LogParseResult result = parser.Parse(
                line,
                context);

            if (result.Success)
            {
                return result;
            }

            if (!string.IsNullOrWhiteSpace(result.FailureReason))
            {
                failures.Add(
                    $"{parser.Name}: {result.FailureReason}");
            }
        }

        string failureReason = failures.Count == 0
            ? "Ningún parser reconoció la línea."
            : string.Join(" | ", failures);

        return LogParseResult.NotMatched(
            Name,
            failureReason);
    }
}