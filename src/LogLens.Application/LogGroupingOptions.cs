namespace LogLens.Application;

public sealed record LogGroupingOptions
{
    public static LogGroupingOptions Default { get; } =
        new();

    public int SampleLimit { get; }

    public bool IncludeUnknownLevels { get; }

    public LogGroupingOptions(
        int sampleLimit = 3,
        bool includeUnknownLevels = true)
    {
        if (sampleLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleLimit),
                "El límite de muestras no puede ser negativo.");
        }

        SampleLimit = sampleLimit;
        IncludeUnknownLevels = includeUnknownLevels;
    }
}