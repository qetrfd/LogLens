using LogLens.Core;
using Xunit;

namespace LogLens.Infrastructure.Tests;

public sealed class RuntimeEnvironmentProviderTests
{
    [Fact]
    public void GetCurrentReturnsExpectedRuntimeInformation()
    {
        RuntimeEnvironmentProvider provider = new();

        RuntimeSnapshot snapshot = provider.GetCurrent();

        Assert.False(string.IsNullOrWhiteSpace(snapshot.OperatingSystem));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.Architecture));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.Framework));
    }
}