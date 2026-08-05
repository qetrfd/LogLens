using LogLens.Core;

namespace LogLens.Desktop.ViewModels;

public sealed record LogLevelFilterOption(
    string Label,
    LogLevel? Value);