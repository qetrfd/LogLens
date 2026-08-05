namespace LogLens.Desktop.Services;

public interface ILogFilePickerService
{
    Task<string?> PickLogFileAsync(
        CancellationToken cancellationToken = default);
}