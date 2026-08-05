using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace LogLens.Desktop.Services;

public sealed class AvaloniaClipboardService
    : IClipboardService
{
    private readonly Func<TopLevel?> _topLevelProvider;

    public AvaloniaClipboardService(
        Func<TopLevel?> topLevelProvider)
    {
        ArgumentNullException.ThrowIfNull(
            topLevelProvider);

        _topLevelProvider =
            topLevelProvider;
    }

    public async Task SetTextAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            text);

        cancellationToken.ThrowIfCancellationRequested();

        TopLevel? topLevel =
            _topLevelProvider();

        if (topLevel is null)
        {
            throw new InvalidOperationException(
                "No se pudo acceder a la ventana principal.");
        }

        IClipboard? clipboard =
            topLevel.Clipboard;

        if (clipboard is null)
        {
            throw new NotSupportedException(
                "El portapapeles no está disponible.");
        }

        await clipboard.SetTextAsync(text);

        cancellationToken.ThrowIfCancellationRequested();
    }
}