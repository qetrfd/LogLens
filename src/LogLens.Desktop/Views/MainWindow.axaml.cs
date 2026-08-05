using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using LogLens.Core;
using LogLens.Desktop.Services;
using LogLens.Desktop.ViewModels;

namespace LogLens.Desktop.Views;

public sealed partial class MainWindow
    : Window
{
    private readonly MainWindowViewModel
        _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        AvaloniaLogFilePickerService filePickerService =
            new(() => this);

        DesktopLogAnalysisService analysisService =
            new();

        _viewModel = new MainWindowViewModel(
            filePickerService,
            analysisService);

        DataContext = _viewModel;

        DragDrop.AddDragEnterHandler(
            this,
            OnDragEnter);

        DragDrop.AddDragLeaveHandler(
            this,
            OnDragLeave);

        DragDrop.AddDragOverHandler(
            this,
            OnDragOver);

        DragDrop.AddDropHandler(
            this,
            OnDrop);

        Closed += OnWindowClosed;
    }

    private void OnDragEnter(
        object? sender,
        DragEventArgs eventArgs)
    {
        bool isSupported =
            GetSupportedFilePath(eventArgs)
            is not null;

        eventArgs.DragEffects =
            isSupported
                ? DragDropEffects.Copy
                : DragDropEffects.None;

        eventArgs.Handled = true;

        _viewModel.SetDropActive(
            isSupported);
    }

    private void OnDragOver(
        object? sender,
        DragEventArgs eventArgs)
    {
        bool isSupported =
            GetSupportedFilePath(eventArgs)
            is not null;

        eventArgs.DragEffects =
            isSupported
                ? DragDropEffects.Copy
                : DragDropEffects.None;

        eventArgs.Handled = true;

        _viewModel.SetDropActive(
            isSupported);
    }

    private void OnDragLeave(
        object? sender,
        DragEventArgs eventArgs)
    {
        eventArgs.Handled = true;

        _viewModel.SetDropActive(false);
    }

    private void OnDrop(
        object? sender,
        DragEventArgs eventArgs)
    {
        string? filePath =
            GetSupportedFilePath(eventArgs);

        eventArgs.DragEffects =
            filePath is null
                ? DragDropEffects.None
                : DragDropEffects.Copy;

        eventArgs.Handled = true;

        _viewModel.SetDropActive(false);

        if (filePath is null)
        {
            return;
        }

        _viewModel.SelectDroppedFile(
            filePath);
    }

    private static string? GetSupportedFilePath(
        DragEventArgs eventArgs)
    {
        IReadOnlyList<IStorageItem>? files =
            eventArgs.DataTransfer.TryGetFiles();

        if (
            files is null ||
            files.Count == 0)
        {
            return null;
        }

        foreach (IStorageItem file in files)
        {
            string? localPath =
                file.TryGetLocalPath();

            if (
                string.IsNullOrWhiteSpace(
                    localPath))
            {
                continue;
            }

            if (
                SupportedLogFileExtensions
                    .IsSupported(localPath))
            {
                return Path.GetFullPath(
                    localPath);
            }
        }

        return null;
    }

    private void OnWindowClosed(
        object? sender,
        EventArgs eventArgs)
    {
        _viewModel.Dispose();
    }
}