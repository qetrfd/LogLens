using LogLens.Application;
using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class StreamingLogFileReaderTests
{
    [Fact]
    public async Task ReadAsyncStreamsLinesInOrder()
    {
        using TemporaryDirectory temporaryDirectory = new();

        string filePath = temporaryDirectory.CreateFilePath(
            "application.log");

        await File.WriteAllTextAsync(
            filePath,
            "Primera línea\n\nTercera línea");

        Guid sourceId = Guid.NewGuid();

        LogReadRequest request = new(
            sourceId,
            filePath,
            progressIntervalLines: 1);

        StreamingLogFileReader reader = new();

        List<RawLogLine> lines = [];

        await foreach (
            RawLogLine line in reader.ReadAsync(request))
        {
            lines.Add(line);
        }

        Assert.Equal(3, lines.Count);
        Assert.Equal(1, lines[0].LineNumber);
        Assert.Equal("Primera línea", lines[0].Text);
        Assert.True(lines[1].IsEmpty);
        Assert.Equal("Tercera línea", lines[2].Text);
    }

    [Fact]
    public async Task ReadAsyncReportsCompletedProgress()
    {
        using TemporaryDirectory temporaryDirectory = new();

        string filePath = temporaryDirectory.CreateFilePath(
            "progress.txt");

        await File.WriteAllTextAsync(
            filePath,
            "Uno\nDos\nTres");

        List<LogReadProgress> events = [];

        ImmediateProgress<LogReadProgress> progress = new(
            value => events.Add(value));

        StreamingLogFileReader reader = new();

        await ConsumeAsync(
            reader.ReadAsync(
                new LogReadRequest(
                    Guid.NewGuid(),
                    filePath,
                    progressIntervalLines: 1),
                progress));

        Assert.NotEmpty(events);
        Assert.True(events[^1].IsCompleted);
        Assert.Equal(100d, events[^1].Percentage);
        Assert.Equal(3, events[^1].LinesRead);
    }

    [Fact]
    public async Task ReadAsyncRejectsUnsupportedExtension()
    {
        using TemporaryDirectory temporaryDirectory = new();

        string filePath = temporaryDirectory.CreateFilePath(
            "events.csv");

        await File.WriteAllTextAsync(
            filePath,
            "message");

        StreamingLogFileReader reader = new();

        await Assert.ThrowsAsync<NotSupportedException>(
            () => ConsumeAsync(
                reader.ReadAsync(
                    new LogReadRequest(
                        Guid.NewGuid(),
                        filePath))));
    }

    [Fact]
    public async Task ReadAsyncRejectsLinesOverConfiguredLimit()
    {
        using TemporaryDirectory temporaryDirectory = new();

        string filePath = temporaryDirectory.CreateFilePath(
            "large.log");

        await File.WriteAllTextAsync(
            filePath,
            "123456");

        StreamingLogFileReader reader = new();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => ConsumeAsync(
                reader.ReadAsync(
                    new LogReadRequest(
                        Guid.NewGuid(),
                        filePath,
                        maximumLineLength: 5))));
    }

    private static async Task ConsumeAsync(
        IAsyncEnumerable<RawLogLine> lines)
    {
        await foreach (RawLogLine _ in lines)
        {
        }
    }

    private sealed class ImmediateProgress<T>
        : IProgress<T>
    {
        private readonly Action<T> _action;

        public ImmediateProgress(Action<T> action)
        {
            _action = action;
        }

        public void Report(T value)
        {
            _action(value);
        }
    }

    private sealed class TemporaryDirectory
        : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"loglens-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string CreateFilePath(string fileName)
        {
            return System.IO.Path.Combine(
                Path,
                fileName);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(
                    Path,
                    recursive: true);
            }
        }
    }
}
