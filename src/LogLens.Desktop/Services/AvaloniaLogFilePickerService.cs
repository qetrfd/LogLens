using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LogLens.Core;

namespace LogLens.Desktop.Services;

public sealed class AvaloniaLogFilePickerService
    : ILogFilePickerService
{
    private readonly Func<TopLevel?> _topLevelProvider;

    public AvaloniaLogFilePickerService(
        Func<TopLevel?> topLevelProvider)
    {
        ArgumentNullException.ThrowIfNull(
            topLevelProvider);

        _topLevelProvider = topLevelProvider;
    }

    public async Task<string?> PickLogFileAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TopLevel? topLevel =
            _topLevelProvider();

        if (topLevel is null)
        {
            throw new InvalidOperationException(
                "No se pudo obtener la ventana principal.");
        }

        if (!topLevel.StorageProvider.CanOpen)
        {
            throw new NotSupportedException(
                "El selector de archivos no está disponible.");
        }

        FilePickerFileType logFiles = new(
            "Archivos de LogLens")
        {
            Patterns =
            [
                "*.log",
                "*.txt",
                "*.jsonl",
                "*.ndjson"
            ]
        };

        FilePickerOpenOptions options = new()
        {
            Title = "Seleccionar archivo de logs",
            AllowMultiple = false,
            FileTypeFilter =
            [
                logFiles
            ]
        };

        IReadOnlyList<IStorageFile> files =
            await topLevel.StorageProvider
                .OpenFilePickerAsync(options);

        cancellationToken.ThrowIfCancellationRequested();

        if (files.Count == 0)
        {
            return null;
        }

        using IStorageFile selectedFile =
            files[0];

        string? localPath =
            selectedFile.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(localPath))
        {
            throw new NotSupportedException(
                "El archivo seleccionado no tiene una ruta local accesible.");
        }

        if (!SupportedLogFileExtensions.IsSupported(localPath))
        {
            throw new NotSupportedException(
                "El formato del archivo no es compatible con LogLens.");
        }

        return Path.GetFullPath(localPath);
    }
}