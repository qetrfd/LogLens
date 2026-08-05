using LogLens.Core;

namespace LogLens.Application;

public interface IIncidentDiagnosticRule
{
    string Id { get; }

    string Name { get; }

    int Order { get; }

    IncidentDiagnosis? Evaluate(
        IncidentDiagnosticContext context);
}