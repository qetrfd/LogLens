using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LogLens.Core;

namespace LogLens.Desktop.Converters;

public sealed class IncidentPriorityBrushConverter
    : IValueConverter
{
    private static readonly IBrush CriticalBrush =
        new SolidColorBrush(
            Color.Parse("#991B1B"));

    private static readonly IBrush HighBrush =
        new SolidColorBrush(
            Color.Parse("#C2410C"));

    private static readonly IBrush MediumBrush =
        new SolidColorBrush(
            Color.Parse("#A16207"));

    private static readonly IBrush LowBrush =
        new SolidColorBrush(
            Color.Parse("#0369A1"));

    private static readonly IBrush NoneBrush =
        new SolidColorBrush(
            Color.Parse("#475569"));

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value switch
        {
            IncidentPriority.Critical =>
                CriticalBrush,

            IncidentPriority.High =>
                HighBrush,

            IncidentPriority.Medium =>
                MediumBrush,

            IncidentPriority.Low =>
                LowBrush,

            _ =>
                NoneBrush
        };
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}