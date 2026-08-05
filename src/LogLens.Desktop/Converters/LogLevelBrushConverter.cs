using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LogLens.Core;

namespace LogLens.Desktop.Converters;

public sealed class LogLevelBrushConverter
    : IValueConverter
{
    private static readonly IBrush CriticalBrush =
        new SolidColorBrush(
            Color.Parse("#991B1B"));

    private static readonly IBrush ErrorBrush =
        new SolidColorBrush(
            Color.Parse("#B91C1C"));

    private static readonly IBrush WarningBrush =
        new SolidColorBrush(
            Color.Parse("#A16207"));

    private static readonly IBrush InformationBrush =
        new SolidColorBrush(
            Color.Parse("#0369A1"));

    private static readonly IBrush DebugBrush =
        new SolidColorBrush(
            Color.Parse("#6D28D9"));

    private static readonly IBrush TraceBrush =
        new SolidColorBrush(
            Color.Parse("#0F766E"));

    private static readonly IBrush UnknownBrush =
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
            LogLevel.Critical =>
                CriticalBrush,

            LogLevel.Error =>
                ErrorBrush,

            LogLevel.Warning =>
                WarningBrush,

            LogLevel.Information =>
                InformationBrush,

            LogLevel.Debug =>
                DebugBrush,

            LogLevel.Trace =>
                TraceBrush,

            _ =>
                UnknownBrush
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