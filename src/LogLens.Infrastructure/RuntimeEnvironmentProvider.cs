using System.Runtime.InteropServices;
using LogLens.Application;
using LogLens.Core;

namespace LogLens.Infrastructure;

public sealed class RuntimeEnvironmentProvider : IRuntimeEnvironmentProvider
{
    public RuntimeSnapshot GetCurrent()
    {
        return new RuntimeSnapshot(
            RuntimeInformation.OSDescription.Trim(),
            RuntimeInformation.OSArchitecture.ToString(),
            RuntimeInformation.FrameworkDescription.Trim());
    }
}