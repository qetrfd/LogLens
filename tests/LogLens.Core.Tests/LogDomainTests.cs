using LogLens.Core;
using Xunit;

namespace LogLens.Core.Tests;

public sealed class LogDomainTests
{
    [Fact]
    public void LogSourceStoresNormalizedValues()
    {
        LogSource source = new(
            Guid.NewGuid(),
            "  API principal  ",
            "  /var/log/api.log  ",
            LogSourceKind.GenericFile,
            false);

        Assert.Equal("API principal", source.Name);
        Assert.Equal("/var/log/api.log", source.Location);
        Assert.Equal(LogSourceKind.GenericFile, source.Kind);
        Assert.False(source.IsLive);
    }

    [Fact]
    public void LogSourceRejectsEmptyIdentifier()
    {
        Assert.Throws<ArgumentException>(() =>
            new LogSource(
                Guid.Empty,
                "API",
                "/var/log/api.log",
                LogSourceKind.GenericFile,
                false));
    }

    [Fact]
    public void LogEntryCopiesMetadataAndNormalizesOptionalValues()
    {
        Dictionary<string, string> metadata = new()
        {
            ["Environment"] = "Production"
        };

        LogEntry entry = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            15,
            DateTimeOffset.UtcNow,
            LogLevel.Error,
            "  Connection refused  ",
            "Connection refused at {ip}:{port}",
            "2026-08-02 ERROR Connection refused",
            "  API  ",
            "  SocketException  ",
            503,
            245,
            "  request-123  ",
            metadata);

        metadata["Environment"] = "Development";

        Assert.Equal(15, entry.LineNumber);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("Connection refused", entry.Message);
        Assert.Equal("API", entry.Service);
        Assert.Equal("SocketException", entry.ExceptionType);
        Assert.Equal("request-123", entry.CorrelationId);
        Assert.Equal("Production", entry.Metadata["Environment"]);
    }

    [Fact]
    public void LogEntryRejectsInvalidLineNumber()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LogEntry(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                null,
                LogLevel.Unknown,
                "Mensaje",
                "Mensaje",
                "Mensaje"));
    }

    [Fact]
    public void LogPatternLimitsStoredSamples()
    {
        DateTimeOffset firstSeen = DateTimeOffset.UtcNow;
        DateTimeOffset lastSeen = firstSeen.AddMinutes(2);

        LogPattern pattern = new(
            Guid.NewGuid(),
            "connection-refused-{ip}",
            "Connection refused",
            LogLevel.Error,
            8,
            firstSeen,
            lastSeen,
            [
                "Ejemplo 1",
                "Ejemplo 2",
                "Ejemplo 3",
                "Ejemplo 4",
                "Ejemplo 5",
                "Ejemplo 6"
            ]);

        Assert.Equal(8, pattern.Occurrences);
        Assert.Equal(5, pattern.Samples.Count);
    }

    [Fact]
    public void LogPatternRejectsInvertedTimeWindow()
    {
        DateTimeOffset firstSeen = DateTimeOffset.UtcNow;
        DateTimeOffset lastSeen = firstSeen.AddMinutes(-1);

        Assert.Throws<ArgumentException>(() =>
            new LogPattern(
                Guid.NewGuid(),
                "timeout",
                "Request timeout",
                LogLevel.Error,
                1,
                firstSeen,
                lastSeen));
    }

    [Fact]
    public void IncidentRequiresAtLeastOnePattern()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new Incident(
                Guid.NewGuid(),
                "API no disponible",
                "La API rechazó múltiples conexiones.",
                IncidentSeverity.Critical,
                IncidentStatus.Open,
                0.95,
                [],
                now,
                now));
    }

    [Fact]
    public void IncidentStoresDiagnosticInformation()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Guid patternId = Guid.NewGuid();

        Incident incident = new(
            Guid.NewGuid(),
            "  Base de datos no disponible  ",
            "  Se detectaron conexiones rechazadas.  ",
            IncidentSeverity.High,
            IncidentStatus.Investigating,
            0.87,
            [patternId],
            now,
            now,
            "  El servicio de base de datos está detenido.  ",
            "  Verificar el host, puerto y estado del servicio.  ");

        Assert.Equal("Base de datos no disponible", incident.Title);
        Assert.Equal(IncidentSeverity.High, incident.Severity);
        Assert.Equal(IncidentStatus.Investigating, incident.Status);
        Assert.Equal(0.87, incident.Confidence);
        Assert.Contains(patternId, incident.PatternIds);
        Assert.Equal(
            "El servicio de base de datos está detenido.",
            incident.ProbableCause);
    }

    [Fact]
    public void AnalysisSessionReportsCompletion()
    {
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        DateTimeOffset completedAt = startedAt.AddMinutes(3);

        AnalysisSession session = new(
            Guid.NewGuid(),
            "Análisis de producción",
            [Guid.NewGuid()],
            startedAt,
            completedAt,
            1500,
            38,
            4);

        Assert.True(session.IsCompleted);
        Assert.Equal(1500, session.TotalEntries);
        Assert.Equal(38, session.PatternCount);
        Assert.Equal(4, session.IncidentCount);
    }

    [Fact]
    public void AnalysisSessionRejectsNegativeCounters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AnalysisSession(
                Guid.NewGuid(),
                "Análisis",
                [Guid.NewGuid()],
                DateTimeOffset.UtcNow,
                null,
                -1,
                0,
                0));
    }
}
