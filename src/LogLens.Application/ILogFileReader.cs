using LogLens.Core;

namespace LogLens.Application;

public interface ILogFileReader
{
    IAsyncEnumerable<RawLogLine> ReadAsync(
        LogReadRequest request,
        IProgress<LogReadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
