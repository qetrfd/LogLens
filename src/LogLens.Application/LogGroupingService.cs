using LogLens.Core;

namespace LogLens.Application;

public sealed class LogGroupingService
{
    private readonly ILogFingerprintGenerator
        _fingerprintGenerator;

    public LogGroupingService(
        ILogFingerprintGenerator fingerprintGenerator)
    {
        ArgumentNullException.ThrowIfNull(
            fingerprintGenerator);

        _fingerprintGenerator =
            fingerprintGenerator;
    }

    public async Task<LogGroupingResult> GroupAsync(
        IAsyncEnumerable<ParsedLogLine> lines,
        LogGroupingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);

        LogGroupingOptions effectiveOptions =
            options ?? LogGroupingOptions.Default;

        Dictionary<string, LogGroupAccumulator>
            accumulators =
                new(StringComparer.OrdinalIgnoreCase);

        long totalEntries = 0;
        long groupedEntries = 0;
        long excludedEntries = 0;

        await foreach (
            ParsedLogLine line in lines
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            totalEntries++;

            if (
                !effectiveOptions.IncludeUnknownLevels &&
                line.Level == LogLevel.Unknown)
            {
                excludedEntries++;
                continue;
            }

            LogFingerprint fingerprint =
                _fingerprintGenerator.Generate(line);

            if (
                !accumulators.TryGetValue(
                    fingerprint.Value,
                    out LogGroupAccumulator? accumulator))
            {
                accumulator = new LogGroupAccumulator(
                    fingerprint,
                    effectiveOptions.SampleLimit);

                accumulators.Add(
                    fingerprint.Value,
                    accumulator);
            }

            accumulator.Add(line);
            groupedEntries++;
        }

        LogGroupSummary[] groups =
            accumulators.Values
                .Select(
                    accumulator =>
                        accumulator.CreateSummary())
                .OrderByDescending(
                    group => group.OccurrenceCount)
                .ThenByDescending(
                    group =>
                        GetSeverity(
                            group.HighestLevel))
                .ThenBy(
                    group =>
                        group.RepresentativeMessage,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        return new LogGroupingResult(
            totalEntries,
            groupedEntries,
            excludedEntries,
            groups,
            DateTimeOffset.UtcNow);
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