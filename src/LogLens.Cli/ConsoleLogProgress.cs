using LogLens.Application;

namespace LogLens.Cli;

internal sealed class ConsoleLogProgress
    : IProgress<LogReadProgress>
{
    private int _lastPercentage = -1;

    public void Report(LogReadProgress value)
    {
        int percentage = (int)Math.Floor(value.Percentage);

        if (
            percentage == _lastPercentage &&
            !value.IsCompleted)
        {
            return;
        }

        _lastPercentage = percentage;

        Console.Write(
            $"\rLeyendo: {percentage,3}% · " +
            $"{value.LinesRead:N0} líneas");

        if (value.IsCompleted)
        {
            Console.WriteLine();
        }
    }
}
