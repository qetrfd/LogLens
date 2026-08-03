using LogLens.Core;

namespace LogLens.Application;

internal sealed class LogGroupAccumulator
{
    private readonly int _sampleLimit;

    private readonly HashSet<string> _services =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _exceptionTypes =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<int> _statusCodes = [];

    private readonly List<LogGroupSample> _samples = [];

    private DateTimeOffset? _firstSeen;

    private DateTimeOffset? _lastSeen;

    private LogLevel _highestLevel =
        LogLevel.Unknown;

    private string? _representativeMessage;

    public LogFingerprint Fingerprint { get; }

    public long OccurrenceCount { get; private set; }

    public LogGroupAccumulator(
        LogFingerprint fingerprint,
        int sampleLimit)
    {
        ArgumentNullException.ThrowIfNull(fingerprint);

        if (sampleLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleLimit));
        }

        Fingerprint = fingerprint;
        _sampleLimit = sampleLimit;
    }

    public void Add(ParsedLogLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        OccurrenceCount++;

        _representativeMessage ??=
            line.Message;

        if (
            GetSeverity(line.Level) >
            GetSeverity(_highestLevel))
        {
            _highestLevel = line.Level;
        }

        if (line.Timestamp.HasValue)
        {
            DateTimeOffset timestamp =
                line.Timestamp.Value;

            if (
                !_firstSeen.HasValue ||
                timestamp < _firstSeen.Value)
            {
                _firstSeen = timestamp;
            }

            if (
                !_lastSeen.HasValue ||
                timestamp > _lastSeen.Value)
            {
                _lastSeen = timestamp;
            }
        }

        if (!string.IsNullOrWhiteSpace(line.Service))
        {
            _services.Add(line.Service);
        }

        if (!string.IsNullOrWhiteSpace(line.ExceptionType))
        {
            _exceptionTypes.Add(line.ExceptionType);
        }

        if (line.StatusCode.HasValue)
        {
            _statusCodes.Add(
                line.StatusCode.Value);
        }

        if (_samples.Count < _sampleLimit)
        {
            _samples.Add(
                LogGroupSample.From(line));
        }
    }

    public LogGroupSummary CreateSummary()
    {
        if (
            OccurrenceCount == 0 ||
            string.IsNullOrWhiteSpace(
                _representativeMessage))
        {
            throw new InvalidOperationException(
                "No se puede crear un resumen de un grupo vacío.");
        }

        return new LogGroupSummary(
            Fingerprint,
            OccurrenceCount,
            _firstSeen,
            _lastSeen,
            _highestLevel,
            _representativeMessage,
            _services,
            _exceptionTypes,
            _statusCodes,
            _samples);
    }

    private static int GetSeverity(
        LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => 1,
            LogLevel.Debug => 2,
            LogLevel.Information => 3,
            LogLevel.Warning => 4,
            LogLevel.Error => 5,
            LogLevel.Critical => 6,
            _ => 0
        };
    }
}