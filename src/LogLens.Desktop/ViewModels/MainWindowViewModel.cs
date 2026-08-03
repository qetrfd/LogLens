using LogLens.Application;
using LogLens.Infrastructure;

namespace LogLens.Desktop.ViewModels;

public sealed class MainWindowViewModel
{
    private readonly StartupSummary _summary;

    public MainWindowViewModel()
    {
        StartupSummaryService startupService = new(
            new RuntimeEnvironmentProvider());

        _summary = startupService.Create();
    }

    public string WindowTitle =>
        $"{_summary.Product.Name} {_summary.Product.Version}";

    public string ProductName =>
        _summary.Product.Name;

    public string Version =>
        $"Versión {_summary.Product.Version}";

    public string Description =>
        _summary.Product.Description;

    public string CurrentPhase =>
        "Fase 3 · Lectura progresiva";

    public string RuntimeDescription =>
        $"{_summary.Runtime.OperatingSystem} · {_summary.Runtime.Architecture}";

    public string FrameworkDescription =>
        _summary.Runtime.Framework;
}