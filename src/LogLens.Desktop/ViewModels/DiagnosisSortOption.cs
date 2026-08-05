using LogLens.Application;

namespace LogLens.Desktop.ViewModels;

public sealed record DiagnosisSortOption(
    string Label,
    IncidentDiagnosisSortOrder Value);