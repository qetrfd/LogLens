using LogLens.Core;

namespace LogLens.Application;

public sealed class LogFileInspectionService
{
    private readonly ILogFileReader _reader;

    public LogFileInspectionService(ILogFileReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        _reader = reader;
    }

    public async Task<LogFileInspectionResult> InspectAsync(
        LogReadRequest request,
        int previewLimit = 10,
        IProgress<LogReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (previewLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previewLimit),
                "El límite de vista previa no puede ser negativo.");
        }

        List<RawLogLine> preview = [];
        long totalLines = 0;
        long emptyLines = 0;
        int longestLineLength = 0;

        await foreach (
            RawLogLine line in _reader
                .ReadAsync(request, progress, cancellationToken)
                .ConfigureAwait(false))
        {
            totalLines++;

            if (line.IsEmpty)
            {
                emptyLines++;
            }

            longestLineLength = Math.Max(
                longestLineLength,
                line.Text.Length);

            if (preview.Count < previewLimit)
            {
                preview.Add(line);
            }
        }

        return new LogFileInspectionResult(
            request.SourceId,
            request.FilePath,
            totalLines,
            emptyLines,
            longestLineLength,
            preview,
            DateTimeOffset.UtcNow);
    }
}
