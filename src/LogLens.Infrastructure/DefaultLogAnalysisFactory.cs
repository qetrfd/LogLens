using LogLens.Application;

namespace LogLens.Infrastructure;

public static class DefaultLogAnalysisFactory
{
    public static LogFileGroupingService
        CreateFileGroupingService()
    {
        return new LogFileGroupingService(
            new StreamingLogFileReader(),
            DefaultLogParserFactory.Create(),
            new DefaultLogFingerprintGenerator());
    }

    public static LogFileDiagnosticService
        CreateFileDiagnosticService()
    {
        return new LogFileDiagnosticService(
            CreateFileGroupingService(),
            DefaultIncidentDiagnosticFactory
                .CreateService());
    }
}