using System.Globalization;
using LogLens.Core;

namespace LogLens.Desktop.ViewModels;

public sealed class LogSampleItemViewModel
{
    public LogGroupSample Model { get; }

    public long LineNumber =>
        Model.LineNumber;

    public string TimestampText { get; }

    public string LevelText =>
        Model.Level.ToString();

    public string Message =>
        Model.Message;

    public string ServiceText { get; }

    public string ExceptionText { get; }

    public string StatusCodeText { get; }

    public string CompleteText =>
        string.Join(
            Environment.NewLine,
            [
                $"Línea: {LineNumber}",
                $"Fecha: {TimestampText}",
                $"Nivel: {LevelText}",
                $"Servicio: {ServiceText}",
                $"Excepción: {ExceptionText}",
                $"Código HTTP: {StatusCodeText}",
                $"Mensaje: {Message}"
            ]);

    public LogSampleItemViewModel(
        LogGroupSample model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Model = model;

        TimestampText =
            model.Timestamp?.ToString(
                "yyyy-MM-dd HH:mm:ss.fff zzz",
                CultureInfo.InvariantCulture)
            ?? "Sin fecha";

        ServiceText =
            string.IsNullOrWhiteSpace(
                model.Service)
                ? "Sin servicio"
                : model.Service;

        ExceptionText =
            string.IsNullOrWhiteSpace(
                model.ExceptionType)
                ? "Sin excepción"
                : model.ExceptionType;

        StatusCodeText =
            model.StatusCode?.ToString(
                CultureInfo.InvariantCulture)
            ?? "Sin código HTTP";
    }
}