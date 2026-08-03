using System.Runtime.CompilerServices;
using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class LogFileInspectionServiceTests
{
    [Fact]
    public async Task InspectAsyncCreatesSummaryAndPreview()
    {
        Guid sourceId = Guid.NewGuid();

        RawLogLine[] lines =
        [
            new(sourceId, 1, "Primera línea"),
            new(sourceId, 2, ""),
            new(sourceId, 3, "Tercera línea extensa")
        ];

        LogFileInspectionService service = new(
            new FakeLogFileReader(lines));

        LogReadRequest request = new(
            sourceId,
            "example.log");

        LogFileInspectionResult result =
            await service.InspectAsync(
                request,
                previewLimit: 2);

        Assert.Equal(3, result.TotalLines);
        Assert.Equal(1, result.EmptyLines);
        Assert.Equal(21, result.LongestLineLength);
        Assert.Equal(2, result.Preview.Count);

        Assert.Equal(
            "Primera línea",
            result.Preview[0].Text);
    }

    [Fact]
    public async Task InspectAsyncCanDisablePreview()
    {
        Guid sourceId = Guid.NewGuid();

        LogFileInspectionService service = new(
            new FakeLogFileReader(
            [
                new RawLogLine(
                    sourceId,
                    1,
                    "Mensaje")
            ]));

        LogFileInspectionResult result =
            await service.InspectAsync(
                new LogReadRequest(
                    sourceId,
                    "example.log"),
                previewLimit: 0);

        Assert.Empty(result.Preview);
        Assert.Equal(1, result.TotalLines);
    }

    [Fact]
    public async Task InspectAsyncRejectsNegativePreviewLimit()
    {
        Guid sourceId = Guid.NewGuid();

        LogFileInspectionService service = new(
            new FakeLogFileReader([]));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () =>
            {
                await service.InspectAsync(
                    new LogReadRequest(
                        sourceId,
                        "example.log"),
                    previewLimit: -1);
            });
    }

    private sealed class FakeLogFileReader
        : ILogFileReader
    {
        private readonly IReadOnlyList<RawLogLine> _lines;

        public FakeLogFileReader(
            IReadOnlyList<RawLogLine> lines)
        {
            _lines = lines;
        }

        public async IAsyncEnumerable<RawLogLine> ReadAsync(
            LogReadRequest request,
            IProgress<LogReadProgress>? progress = null,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
        {
            long count = 0;

            foreach (RawLogLine line in _lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Yield();

                count++;

                yield return line;
            }

            progress?.Report(
                new LogReadProgress(
                    request.FilePath,
                    count,
                    count,
                    count,
                    true));
        }
    }
}
