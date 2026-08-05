using LogLens.Core;
using Xunit;

namespace LogLens.Core.Tests;

public sealed class LogLensProductTests
{
    [Fact]
    public void CurrentContainsExpectedProductInformation()
    {
        ApplicationIdentity product =
            LogLensProduct.Current;

        Assert.Equal(
            "LogLens",
            product.Name);

        Assert.Equal(
            "0.7.0",
            product.Version);

        Assert.False(
            string.IsNullOrWhiteSpace(
                product.Description));
    }
}