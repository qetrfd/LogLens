namespace LogLens.Desktop.Services;

public interface IClipboardService
{
    Task SetTextAsync(
        string text,
        CancellationToken cancellationToken = default);
}