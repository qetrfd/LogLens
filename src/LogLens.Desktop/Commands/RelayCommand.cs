using System.Windows.Input;

namespace LogLens.Desktop.Commands;

public sealed class RelayCommand
    : ICommand
{
    private readonly Action _execute;

    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(
        Action execute,
        Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(
        object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(
        object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        _execute();
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(
            this,
            EventArgs.Empty);
    }
}