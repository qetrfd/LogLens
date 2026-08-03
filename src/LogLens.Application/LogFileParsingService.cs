using LogLens.Core;

namespace LogLens.Application;

public sealed class LogFileParsingService
{
    private readonly ILogFileReader _reader;

    private readonly LogParsingService _parsingService;

    public LogFileParsingService(
        ILogFileReader reader,
        ILogLineParser parser)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(parser);

        _reader = reader;
        _parsingService = new LogParsingService(parser);
    }

    public async Task<LogFileParsingResult> ParseAsync(
        LogReadRequest request,
        string sourceName,
        int previewLimit = 20,
        IProgress<LogReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        if (previewLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previewLimit),
                "El límite de vista previa no puede ser negativo.");
        }

        LogParserContext context = new(
            request.SourceId,
            sourceName,
            request.FilePath);

        Dictionary<LogLevel, long> levelCounts = [];
        Dictionary<string, long> parserCounts =
            new(StringComparer.OrdinalIgnoreCase);

        List<ParsedLogLine> preview = [];

        long totalLines = 0;
        long parsedLines = 0;
        long unparsedLines = 0;

        await foreach (
            RawLogLine line in _reader
                .ReadAsync(
                    request,
                    progress,
                    cancellationToken)
                .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            totalLines++;

            LogParseResult result =
                _parsingService.Parse(
                    line,
                    context);

            if (
                !result.Success ||
                result.ParsedLine is null)
            {
                unparsedLines++;
                continue;
            }

            ParsedLogLine parsedLine =
                result.ParsedLine;

            parsedLines++;

            levelCounts.TryGetValue(
                parsedLine.Level,
                out long levelCount);

            levelCounts[parsedLine.Level] =
                levelCount + 1;

            parserCounts.TryGetValue(
                result.ParserName,
                out long parserCount);

            parserCounts[result.ParserName] =
                parserCount + 1;

            if (preview.Count < previewLimit)
            {
                preview.Add(parsedLine);
            }
        }

        return new LogFileParsingResult(
            request.SourceId,
            sourceName,
            request.FilePath,
            totalLines,
            parsedLines,
            unparsedLines,
            levelCounts,
            parserCounts,
            preview,
            DateTimeOffset.UtcNow);
    }
}