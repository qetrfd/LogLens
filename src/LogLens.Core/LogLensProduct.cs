namespace LogLens.Core;

public static class LogLensProduct
{
    public static ApplicationIdentity Current { get; } = new(
        "LogLens",
        "0.2.0",
        "Local log analysis and incident diagnostics for developers.");
}
