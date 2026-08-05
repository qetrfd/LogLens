using LogLens.Core;

namespace LogLens.Desktop.ViewModels;

public sealed record PriorityFilterOption(
    string Label,
    IncidentPriority? Value);