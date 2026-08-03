using LogLens.Core;

namespace LogLens.Application;

public sealed class StartupSummaryService
{
    private readonly IRuntimeEnvironmentProvider _runtimeEnvironmentProvider;

    public StartupSummaryService(
        IRuntimeEnvironmentProvider runtimeEnvironmentProvider)
    {
        ArgumentNullException.ThrowIfNull(runtimeEnvironmentProvider);

        _runtimeEnvironmentProvider = runtimeEnvironmentProvider;
    }

    public StartupSummary Create()
    {
        return new StartupSummary(
            LogLensProduct.Current,
            _runtimeEnvironmentProvider.GetCurrent(),
            DateTimeOffset.UtcNow);
    }
}
