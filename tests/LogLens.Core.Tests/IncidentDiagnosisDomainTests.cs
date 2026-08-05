using LogLens.Core;
using Xunit;

namespace LogLens.Core.Tests;

public sealed class IncidentDiagnosisDomainTests
{
    [Fact]
    public void DiagnosticEvidenceNormalizesCode()
    {
        DiagnosticEvidence evidence = new(
            "  HTTP_STATUS_CODE  ",
            "Código HTTP",
            "503");

        Assert.Equal(
            "http-status-code",
            evidence.Code);

        Assert.Equal(
            "Código HTTP",
            evidence.Label);

        Assert.Equal(
            "503",
            evidence.Value);
    }

    [Fact]
    public void IncidentDiagnosisNormalizesValues()
    {
        DateTimeOffset detectedAt = new(
            2026,
            8,
            4,
            22,
            0,
            0,
            TimeSpan.Zero);

        IncidentDiagnosis diagnosis = new(
            "  CONNECTION_FAILURE  ",
            "  Fallo de conexión  ",
            "  No fue posible conectar con el servicio.  ",
            IncidentPriority.High,
            92.5,
            "  ABCDEF123456  ",
            [
                new DiagnosticEvidence(
                    "connection-error",
                    "Error",
                    "Connection refused")
            ],
            [
                "  Revisar el servicio de destino.  ",
                "Revisar el servicio de destino.",
                "Verificar la red."
            ],
            detectedAt);

        Assert.Equal(
            "connection-failure",
            diagnosis.RuleId);

        Assert.Equal(
            "Fallo de conexión",
            diagnosis.Title);

        Assert.Equal(
            "No fue posible conectar con el servicio.",
            diagnosis.Summary);

        Assert.Equal(
            "abcdef123456",
            diagnosis.Fingerprint);

        Assert.Equal(
            92.5,
            diagnosis.ConfidencePercentage);

        Assert.Equal(
            2,
            diagnosis.RecommendedActions.Count);

        Assert.True(
            diagnosis.RequiresImmediateAttention);

        Assert.Equal(
            detectedAt,
            diagnosis.DetectedAt);
    }

    [Theory]
    [InlineData(IncidentPriority.None, false)]
    [InlineData(IncidentPriority.Low, false)]
    [InlineData(IncidentPriority.Medium, false)]
    [InlineData(IncidentPriority.High, true)]
    [InlineData(IncidentPriority.Critical, true)]
    public void RequiresImmediateAttentionUsesPriority(
        IncidentPriority priority,
        bool expected)
    {
        IncidentDiagnosis diagnosis = new(
            "test-rule",
            "Test diagnosis",
            "Test summary",
            priority,
            80,
            "test-fingerprint",
            [
                new DiagnosticEvidence(
                    "test",
                    "Test",
                    "Value")
            ],
            [
                "Review the incident."
            ],
            DateTimeOffset.UtcNow);

        Assert.Equal(
            expected,
            diagnosis.RequiresImmediateAttention);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.1)]
    public void IncidentDiagnosisRejectsInvalidConfidence(
        double confidence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IncidentDiagnosis(
                "test-rule",
                "Test diagnosis",
                "Test summary",
                IncidentPriority.Low,
                confidence,
                "test-fingerprint",
                [
                    new DiagnosticEvidence(
                        "test",
                        "Test",
                        "Value")
                ],
                [
                    "Review the incident."
                ],
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IncidentDiagnosisRequiresEvidence()
    {
        Assert.Throws<ArgumentException>(() =>
            new IncidentDiagnosis(
                "test-rule",
                "Test diagnosis",
                "Test summary",
                IncidentPriority.Low,
                80,
                "test-fingerprint",
                [],
                [
                    "Review the incident."
                ],
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IncidentDiagnosisRequiresActions()
    {
        Assert.Throws<ArgumentException>(() =>
            new IncidentDiagnosis(
                "test-rule",
                "Test diagnosis",
                "Test summary",
                IncidentPriority.Low,
                80,
                "test-fingerprint",
                [
                    new DiagnosticEvidence(
                        "test",
                        "Test",
                        "Value")
                ],
                [],
                DateTimeOffset.UtcNow));
    }
}