using System.Runtime.CompilerServices;
using LogLens.Core;

namespace LogLens.Application;

public sealed class LogParsingService
{
    private readonly ILogLineParser _parser;

    public LogParsingService(ILogLineParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        _parser = parser;
    }

    public LogParseResult Parse(
        RawLogLine line,
        LogParserContext context)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(context);

        if (line.SourceId != context.SourceId)
        {
            throw new ArgumentException(
                "La línea y el contexto pertenecen a fuentes diferentes.",
                nameof(context));
        }

        return _parser.Parse(
            line,
            context);
    }

    public async IAsyncEnumerable<LogParseResult> ParseAsync(
        IAsyncEnumerable<RawLogLine> lines,
        LogParserContext context,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(context);

        await foreach (
            RawLogLine line in lines
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return Parse(
                line,
                context);
        }
    }
}