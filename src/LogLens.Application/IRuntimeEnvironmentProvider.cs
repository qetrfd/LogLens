using LogLens.Core;

namespace LogLens.Application;

public interface IRuntimeEnvironmentProvider
{
    RuntimeSnapshot GetCurrent();
}
