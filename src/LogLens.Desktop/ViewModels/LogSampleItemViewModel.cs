using System.Globalization;
using LogLens.Core;

namespace LogLens.Desktop.ViewModels;

public sealed class LogSampleItemViewModel
{
    public long LineNumber { get; }

    public string TimestampText { get; }

    public string LevelText { get; }

    public string Message { get; }

    public string ServiceText { get; }

    public string ExceptionText { get; }

    public string StatusCodeText { get; }

    public LogSampleItemViewModel(
        LogGroupSample sample)
    {
        ArgumentNullException.ThrowIfNull(sample);

        LineNumber = sample.LineNumber;

        TimestampText =
            sample.Timestamp?.ToString(
                "yyyy-MM-dd HH:mm:ss.fff zzz",
                CultureInfo.InvariantCulture)
            ?? "Sin fecha";

        LevelText =
            sample.Level.ToString();

        Message =
            sample.Message;

        ServiceText =
            string.IsNullOrWhiteSpace(sample.Service)
                ? "Sin servicio"
                : sample.Service;

        ExceptionText =
            string.IsNullOrWhiteSpace(
                sample.ExceptionType)
                ? "Sin excepción"
                : sample.ExceptionType;

        StatusCodeText =
            sample.StatusCode?.ToString(
                CultureInfo.InvariantCulture)
            ?? "Sin código HTTP";
    }
}