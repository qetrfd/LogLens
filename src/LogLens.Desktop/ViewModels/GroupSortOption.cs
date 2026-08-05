using LogLens.Application;

namespace LogLens.Desktop.ViewModels;

public sealed record GroupSortOption(
    string Label,
    LogGroupSortOrder Value);