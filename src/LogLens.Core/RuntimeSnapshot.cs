namespace LogLens.Core;

public sealed record RuntimeSnapshot(
    string OperatingSystem,
    string Architecture,
    string Framework);
