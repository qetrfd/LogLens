using LogLens.Core;
using Xunit;

namespace LogLens.Application.Tests;

public sealed class StartupSummaryServiceTests
{
    [Fact]
    public void CreateReturnsProductAndRuntimeInformation()
    {
        FakeRuntimeEnvironmentProvider provider = new();

        StartupSummaryService service = new(provider);

        StartupSummary summary = service.Create();

        Assert.Equal("LogLens", summary.Product.Name);
        Assert.Equal("Test OS", summary.Runtime.OperatingSystem);
        Assert.Equal("Arm64", summary.Runtime.Architecture);
        Assert.Equal(".NET Test", summary.Runtime.Framework);
    }

    private sealed class FakeRuntimeEnvironmentProvider
        : IRuntimeEnvironmentProvider
    {
        public RuntimeSnapshot GetCurrent()
        {
            return new RuntimeSnapshot(
                "Test OS",
                "Arm64",
                ".NET Test");
        }
    }
}
