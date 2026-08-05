using LogLens.Application;

namespace LogLens.Infrastructure;

public static class DefaultIncidentDiagnosticFactory
{
    public static IncidentDiagnosticService CreateService()
    {
        return new IncidentDiagnosticService(
            CreateRules());
    }

    public static IReadOnlyList<IIncidentDiagnosticRule>
        CreateRules()
    {
        return
        [
            new CriticalIncidentDiagnosticRule(),
            new RecurringFailureDiagnosticRule(),
            new HttpFailureDiagnosticRule(),
            new HighLatencyDiagnosticRule(),
            new ConnectionFailureDiagnosticRule()
        ];
    }
}