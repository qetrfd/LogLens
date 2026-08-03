using System.Runtime.CompilerServices;
using LogLens.Core;

namespace LogLens.Application;

public sealed class LogFileGroupingService
{
    private readonly ILogFileReader _reader;

    private readonly LogParsingService _parsingService;

    private readonly LogGroupingService _groupingService;

    public LogFileGroupingService(
        ILogFileReader reader,
        ILogLineParser parser,
        ILogFingerprintGenerator fingerprintGenerator)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(
            fingerprintGenerator);

        _reader = reader;
        _parsingService = new LogParsingService(parser);
        _groupingService = new LogGroupingService(
            fingerprintGenerator);
    }

    public async Task<LogFileGroupingResult> GroupAsync(
        LogReadRequest request,
        string sourceName,
        LogGroupingOptions? options = null,
        IProgress<LogReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        LogParserContext context = new(
            request.SourceId,
            sourceName,
            request.FilePath);

        ParsingCounters counters = new();

        IAsyncEnumerable<RawLogLine> rawLines =
            _reader.ReadAsync(
                request,
                progress,
                cancellationToken);

        IAsyncEnumerable<ParsedLogLine> parsedLines =
            ParseLinesAsync(
                rawLines,
                context,
                counters,
                cancellationToken);

        LogGroupingResult grouping =
            await _groupingService.GroupAsync(
                parsedLines,
                options,
                cancellationToken);

        return new LogFileGroupingResult(
            request.SourceId,
            sourceName,
            request.FilePath,
            counters.TotalLines,
            counters.ParsedLines,
            counters.UnparsedLines,
            grouping,
            DateTimeOffset.UtcNow);
    }

    private async IAsyncEnumerable<ParsedLogLine>
        ParseLinesAsync(
            IAsyncEnumerable<RawLogLine> lines,
            LogParserContext context,
            ParsingCounters counters,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
    {
        await foreach (
            RawLogLine line in lines
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            counters.TotalLines++;

            LogParseResult result =
                _parsingService.Parse(
                    line,
                    context);

            if (
                !result.Success ||
                result.ParsedLine is null)
            {
                counters.UnparsedLines++;
                continue;
            }

            counters.ParsedLines++;

            yield return result.ParsedLine;
        }
    }

    private sealed class ParsingCounters
    {
        public long TotalLines { get; set; }

        public long ParsedLines { get; set; }

        public long UnparsedLines { get; set; }
    }
}