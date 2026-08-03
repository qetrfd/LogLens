using LogLens.Core;

namespace LogLens.Application;

public sealed record StartupSummary(
    ApplicationIdentity Product,
    RuntimeSnapshot Runtime,
    DateTimeOffset StartedAt);
