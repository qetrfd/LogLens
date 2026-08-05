using System.Windows.Input;

namespace LogLens.Desktop.Commands;

public sealed class AsyncRelayCommand
    : ICommand
{
    private readonly Func<Task> _executeAsync;

    private readonly Func<bool>? _canExecute;

    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;

    public AsyncRelayCommand(
        Func<Task> executeAsync,
        Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(
            executeAsync);

        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(
        object? parameter)
    {
        return
            !_isExecuting &&
            (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(
        object? parameter)
    {
        await ExecuteAsync();
    }

    public async Task ExecuteAsync()
    {
        if (!CanExecute(null))
        {
            return;
        }

        _isExecuting = true;
        NotifyCanExecuteChanged();

        try
        {
            await _executeAsync();
        }
        finally
        {
            _isExecuting = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(
            this,
            EventArgs.Empty);
    }
}