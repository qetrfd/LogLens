using System.Globalization;
using LogLens.Core;

namespace LogLens.Desktop.ViewModels;

public sealed class IncidentGroupItemViewModel
{
    public LogGroupSummary Model { get; }

    public LogLevel Level =>
        Model.HighestLevel;

    public string LevelText =>
        Model.HighestLevel.ToString();

    public string Message =>
        Model.RepresentativeMessage;

    public string OccurrenceText =>
        Model.OccurrenceCount == 1
            ? "1 aparición"
            : $"{Model.OccurrenceCount:N0} apariciones";

    public string RecurrenceText =>
        Model.IsRecurring
            ? "Incidente recurrente"
            : "Incidente único";

    public string Fingerprint =>
        Model.Fingerprint.Value;

    public string ShortFingerprint =>
        Shorten(
            Model.Fingerprint.Value,
            18);

    public string NormalizedMessage =>
        Model.Fingerprint.NormalizedMessage;

    public string FirstSeenText =>
        FormatTimestamp(Model.FirstSeen);

    public string LastSeenText =>
        FormatTimestamp(Model.LastSeen);

    public string ActivityWindowText =>
        CreateActivityWindowText(
            Model.FirstSeen,
            Model.LastSeen);

    public string ServicesText =>
        Model.Services.Count == 0
            ? "No identificados"
            : string.Join(
                ", ",
                Model.Services);

    public string ExceptionsText =>
        Model.ExceptionTypes.Count == 0
            ? "No identificadas"
            : string.Join(
                ", ",
                Model.ExceptionTypes);

    public string StatusCodesText =>
        Model.StatusCodes.Count == 0
            ? "No identificados"
            : string.Join(
                ", ",
                Model.StatusCodes);

    public IReadOnlyList<LogSampleItemViewModel>
        Samples { get; }

    public IncidentGroupItemViewModel(
        LogGroupSummary model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Model = model;

        Samples =
            model.Samples
                .Select(
                    sample =>
                        new LogSampleItemViewModel(
                            sample))
                .ToArray();
    }

    private static string FormatTimestamp(
        DateTimeOffset? timestamp)
    {
        return timestamp?.ToString(
            "yyyy-MM-dd HH:mm:ss.fff zzz",
            CultureInfo.InvariantCulture)
            ?? "No disponible";
    }

    private static string CreateActivityWindowText(
        DateTimeOffset? firstSeen,
        DateTimeOffset? lastSeen)
    {
        if (
            !firstSeen.HasValue ||
            !lastSeen.HasValue)
        {
            return "No disponible";
        }

        TimeSpan duration =
            lastSeen.Value -
            firstSeen.Value;

        if (duration.TotalDays >= 1)
        {
            return
                $"{duration.TotalDays:0.##} días";
        }

        if (duration.TotalHours >= 1)
        {
            return
                $"{duration.TotalHours:0.##} horas";
        }

        if (duration.TotalMinutes >= 1)
        {
            return
                $"{duration.TotalMinutes:0.##} minutos";
        }

        return
            $"{Math.Max(0, duration.TotalSeconds):0.##} segundos";
    }

    private static string Shorten(
        string value,
        int maximumLength)
    {
        if (value.Length <= maximumLength)
        {
            return value;
        }

        return value[..maximumLength];
    }
}