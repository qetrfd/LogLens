using System.Runtime.CompilerServices;
using System.Text;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class StreamingLogFileReader : ILogFileReader
{
    private const int BufferSize = 65_536;

    public async IAsyncEnumerable<RawLogLine> ReadAsync(
        LogReadRequest request,
        IProgress<LogReadProgress>? progress = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!File.Exists(request.FilePath))
        {
            throw new FileNotFoundException(
                "No se encontró el archivo de logs.",
                request.FilePath);
        }

        if (!SupportedLogFileExtensions.IsSupported(request.FilePath))
        {
            string supported = string.Join(
                ", ",
                SupportedLogFileExtensions.All);

            throw new NotSupportedException(
                $"El formato no es compatible. Formatos admitidos: {supported}");
        }

        FileInfo fileInfo = new(request.FilePath);
        long totalBytes = fileInfo.Length;

        await using FileStream stream = new(
            request.FilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        using StreamReader reader = new(
            stream,
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: false),
            detectEncodingFromByteOrderMarks: true,
            bufferSize: BufferSize,
            leaveOpen: true);

        long lineNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string? text = await reader
                .ReadLineAsync(cancellationToken)
                .ConfigureAwait(false);

            if (text is null)
            {
                break;
            }

            lineNumber++;

            if (text.Length > request.MaximumLineLength)
            {
                throw new InvalidDataException(
                    $"La línea {lineNumber} supera el límite de " +
                    $"{request.MaximumLineLength} caracteres.");
            }

            yield return new RawLogLine(
                request.SourceId,
                lineNumber,
                text);

            if (
                lineNumber % request.ProgressIntervalLines == 0)
            {
                progress?.Report(
                    new LogReadProgress(
                        request.FilePath,
                        lineNumber,
                        Math.Min(stream.Position, totalBytes),
                        totalBytes,
                        false));
            }
        }

        progress?.Report(
            new LogReadProgress(
                request.FilePath,
                lineNumber,
                totalBytes,
                totalBytes,
                true));
    }
}
