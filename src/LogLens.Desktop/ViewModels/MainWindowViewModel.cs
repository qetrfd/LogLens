using System.Globalization;
using LogLens.Application;
using LogLens.Core;
using LogLens.Desktop.Commands;
using LogLens.Desktop.Services;
using LogLens.Infrastructure;

namespace LogLens.Desktop.ViewModels;

public sealed class MainWindowViewModel
    : ViewModelBase,
      IDisposable
{
    private readonly ILogFilePickerService
        _filePickerService;

    private readonly IDesktopLogAnalysisService
        _analysisService;

    private readonly StartupSummary
        _startupSummary;

    private CancellationTokenSource?
        _analysisCancellation;

    private string? _selectedFilePath;

    private string _selectedFileName =
        "Ningún archivo seleccionado";

    private string _selectedFileMetadata =
        "Formatos: .log, .txt, .jsonl y .ndjson";

    private string _statusText =
        "Selecciona un archivo de logs";

    private string _statusDetail =
        "También puedes arrastrar un archivo hasta la ventana.";

    private bool _isAnalyzing;

    private bool _isDropActive;

    private bool _hasResults;

    private bool _isError;

    private bool _isEmptyResult;

    private double _progressPercentage;

    private string _progressLinesText =
        "0 líneas leídas";

    private string _progressBytesText =
        "0 B";

    private string _totalLinesText =
        "0";

    private string _parsedLinesText =
        "0";

    private string _groupCountText =
        "0";

    private string _diagnosisCountText =
        "0";

    private string _immediateAttentionText =
        "0";

    private string _criticalStatusText =
        "No";

    private string _analysisCompletedText =
        "No analizado";

    private IReadOnlyList<IncidentGroupItemViewModel>
        _groupItems =
            [];

    private IReadOnlyList<DiagnosisItemViewModel>
        _diagnosisItems =
            [];

    private IncidentGroupItemViewModel?
        _selectedGroup;

    private DiagnosisItemViewModel?
        _selectedDiagnosis;

    public string WindowTitle =>
        $"{_startupSummary.Product.Name} " +
        $"{_startupSummary.Product.Version}";

    public string ProductName =>
        _startupSummary.Product.Name;

    public string Version =>
        $"Versión {_startupSummary.Product.Version}";

    public string Description =>
        _startupSummary.Product.Description;

    public string CurrentPhase =>
        "Fase 7 · Interfaz de escritorio funcional";

    public string RuntimeDescription =>
        $"{_startupSummary.Runtime.OperatingSystem} · " +
        $"{_startupSummary.Runtime.Architecture} · " +
        $"{_startupSummary.Runtime.Framework}";

    public string? SelectedFilePath
    {
        get => _selectedFilePath;

        private set
        {
            if (
                SetProperty(
                    ref _selectedFilePath,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasSelectedFile));

                RefreshCommands();
            }
        }
    }

    public string SelectedFileName
    {
        get => _selectedFileName;

        private set =>
            SetProperty(
                ref _selectedFileName,
                value);
    }

    public string SelectedFileMetadata
    {
        get => _selectedFileMetadata;

        private set =>
            SetProperty(
                ref _selectedFileMetadata,
                value);
    }

    public string StatusText
    {
        get => _statusText;

        private set =>
            SetProperty(
                ref _statusText,
                value);
    }

    public string StatusDetail
    {
        get => _statusDetail;

        private set =>
            SetProperty(
                ref _statusDetail,
                value);
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;

        private set
        {
            if (
                SetProperty(
                    ref _isAnalyzing,
                    value))
            {
                RaiseDisplayStateProperties();
                RefreshCommands();
            }
        }
    }

    public bool IsDropActive
    {
        get => _isDropActive;

        private set =>
            SetProperty(
                ref _isDropActive,
                value);
    }

    public bool HasResults
    {
        get => _hasResults;

        private set
        {
            if (
                SetProperty(
                    ref _hasResults,
                    value))
            {
                RaiseDisplayStateProperties();
                RefreshCommands();
            }
        }
    }

    public bool IsError
    {
        get => _isError;

        private set
        {
            if (
                SetProperty(
                    ref _isError,
                    value))
            {
                RaiseDisplayStateProperties();
                RefreshCommands();
            }
        }
    }

    public bool IsEmptyResult
    {
        get => _isEmptyResult;

        private set
        {
            if (
                SetProperty(
                    ref _isEmptyResult,
                    value))
            {
                RaiseDisplayStateProperties();
                RefreshCommands();
            }
        }
    }

    public double ProgressPercentage
    {
        get => _progressPercentage;

        private set =>
            SetProperty(
                ref _progressPercentage,
                Math.Clamp(
                    value,
                    0,
                    100));
    }

    public string ProgressLinesText
    {
        get => _progressLinesText;

        private set =>
            SetProperty(
                ref _progressLinesText,
                value);
    }

    public string ProgressBytesText
    {
        get => _progressBytesText;

        private set =>
            SetProperty(
                ref _progressBytesText,
                value);
    }

    public string TotalLinesText
    {
        get => _totalLinesText;

        private set =>
            SetProperty(
                ref _totalLinesText,
                value);
    }

    public string ParsedLinesText
    {
        get => _parsedLinesText;

        private set =>
            SetProperty(
                ref _parsedLinesText,
                value);
    }

    public string GroupCountText
    {
        get => _groupCountText;

        private set =>
            SetProperty(
                ref _groupCountText,
                value);
    }

    public string DiagnosisCountText
    {
        get => _diagnosisCountText;

        private set =>
            SetProperty(
                ref _diagnosisCountText,
                value);
    }

    public string ImmediateAttentionText
    {
        get => _immediateAttentionText;

        private set =>
            SetProperty(
                ref _immediateAttentionText,
                value);
    }

    public string CriticalStatusText
    {
        get => _criticalStatusText;

        private set =>
            SetProperty(
                ref _criticalStatusText,
                value);
    }

    public string AnalysisCompletedText
    {
        get => _analysisCompletedText;

        private set =>
            SetProperty(
                ref _analysisCompletedText,
                value);
    }

    public IReadOnlyList<IncidentGroupItemViewModel>
        GroupItems
    {
        get => _groupItems;

        private set
        {
            if (
                SetProperty(
                    ref _groupItems,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasGroups));

                RaisePropertyChanged(
                    nameof(HasNoGroups));
            }
        }
    }

    public IReadOnlyList<DiagnosisItemViewModel>
        DiagnosisItems
    {
        get => _diagnosisItems;

        private set
        {
            if (
                SetProperty(
                    ref _diagnosisItems,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasDiagnoses));

                RaisePropertyChanged(
                    nameof(HasNoDiagnoses));
            }
        }
    }

    public IncidentGroupItemViewModel?
        SelectedGroup
    {
        get => _selectedGroup;

        set
        {
            if (
                SetProperty(
                    ref _selectedGroup,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasSelectedGroup));

                RaisePropertyChanged(
                    nameof(HasNoSelectedGroup));
            }
        }
    }

    public DiagnosisItemViewModel?
        SelectedDiagnosis
    {
        get => _selectedDiagnosis;

        set
        {
            if (
                SetProperty(
                    ref _selectedDiagnosis,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasSelectedDiagnosis));

                RaisePropertyChanged(
                    nameof(HasNoSelectedDiagnosis));
            }
        }
    }

    public bool HasSelectedFile =>
        !string.IsNullOrWhiteSpace(
            SelectedFilePath);

    public bool HasGroups =>
        GroupItems.Count > 0;

    public bool HasNoGroups =>
        !HasGroups;

    public bool HasDiagnoses =>
        DiagnosisItems.Count > 0;

    public bool HasNoDiagnoses =>
        !HasDiagnoses;

    public bool HasSelectedGroup =>
        SelectedGroup is not null;

    public bool HasNoSelectedGroup =>
        !HasSelectedGroup;

    public bool HasSelectedDiagnosis =>
        SelectedDiagnosis is not null;

    public bool HasNoSelectedDiagnosis =>
        !HasSelectedDiagnosis;

    public bool ShowStartState =>
        !IsAnalyzing &&
        !HasResults &&
        !IsError &&
        !IsEmptyResult;

    public bool ShowProgressState =>
        IsAnalyzing;

    public bool ShowResultsState =>
        !IsAnalyzing &&
        HasResults;

    public bool ShowErrorState =>
        !IsAnalyzing &&
        IsError;

    public bool ShowEmptyState =>
        !IsAnalyzing &&
        IsEmptyResult;

    public AsyncRelayCommand
        SelectFileCommand { get; }

    public AsyncRelayCommand
        AnalyzeCommand { get; }

    public RelayCommand
        CancelCommand { get; }

    public RelayCommand
        ClearCommand { get; }

    public MainWindowViewModel(
        ILogFilePickerService filePickerService,
        IDesktopLogAnalysisService analysisService)
    {
        ArgumentNullException.ThrowIfNull(
            filePickerService);

        ArgumentNullException.ThrowIfNull(
            analysisService);

        _filePickerService =
            filePickerService;

        _analysisService =
            analysisService;

        StartupSummaryService startupService = new(
            new RuntimeEnvironmentProvider());

        _startupSummary =
            startupService.Create();

        SelectFileCommand = new AsyncRelayCommand(
            SelectFileAsync,
            () => !IsAnalyzing);

        AnalyzeCommand = new AsyncRelayCommand(
            AnalyzeSelectedFileAsync,
            () =>
                HasSelectedFile &&
                !IsAnalyzing);

        CancelCommand = new RelayCommand(
            CancelAnalysis,
            () => IsAnalyzing);

        ClearCommand = new RelayCommand(
            Clear,
            () =>
                !IsAnalyzing &&
                (
                    HasSelectedFile ||
                    HasResults ||
                    IsError ||
                    IsEmptyResult
                ));
    }

    public void SetDropActive(
        bool value)
    {
        IsDropActive =
            value &&
            !IsAnalyzing;
    }

    public void SelectDroppedFile(
        string filePath)
    {
        IsDropActive = false;

        if (IsAnalyzing)
        {
            StatusText =
                "Hay un análisis en curso";

            StatusDetail =
                "Cancela el análisis actual antes de seleccionar otro archivo.";

            return;
        }

        SetSelectedFile(filePath);
    }

    private async Task SelectFileAsync()
    {
        try
        {
            string? filePath =
                await _filePickerService
                    .PickLogFileAsync();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            SetSelectedFile(filePath);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            SetError(
                "No se pudo seleccionar el archivo",
                exception.Message);
        }
    }

    private void SetSelectedFile(
        string filePath)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                filePath);

            string fullPath =
                Path.GetFullPath(
                    filePath.Trim());

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    "El archivo seleccionado ya no existe.",
                    fullPath);
            }

            if (
                !SupportedLogFileExtensions.IsSupported(
                    fullPath))
            {
                throw new NotSupportedException(
                    "LogLens admite archivos .log, .txt, .jsonl y .ndjson.");
            }

            FileInfo fileInfo =
                new(fullPath);

            ResetAnalysisOutput();

            SelectedFilePath =
                fullPath;

            SelectedFileName =
                fileInfo.Name;

            SelectedFileMetadata =
                $"{FormatBytes(fileInfo.Length)} · " +
                $"Modificado " +
                $"{fileInfo.LastWriteTime.ToString(
                    "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture)}";

            IsError = false;
            IsEmptyResult = false;

            StatusText =
                "Archivo listo para analizar";

            StatusDetail =
                "Presiona Analizar para detectar incidentes y diagnósticos.";
        }
        catch (Exception exception)
        {
            SetError(
                "No se pudo cargar el archivo",
                exception.Message);
        }

        RefreshCommands();
    }

    private async Task AnalyzeSelectedFileAsync()
    {
        string? filePath =
            SelectedFilePath;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        ResetAnalysisOutput();

        CancellationTokenSource cancellationSource =
            new();

        _analysisCancellation =
            cancellationSource;

        IsAnalyzing = true;
        IsError = false;
        IsEmptyResult = false;
        IsDropActive = false;

        StatusText =
            "Analizando archivo";

        StatusDetail =
            "Leyendo y agrupando las entradas del log.";

        ProgressPercentage = 0;

        ProgressLinesText =
            "0 líneas leídas";

        ProgressBytesText =
            "Preparando lectura";

        Progress<LogReadProgress> progress = new(
            ReportProgress);

        try
        {
            LogFileDiagnosticResult result =
                await _analysisService.AnalyzeAsync(
                    filePath,
                    progress,
                    cancellationSource.Token);

            cancellationSource.Token
                .ThrowIfCancellationRequested();

            ApplyResult(result);
        }
        catch (OperationCanceledException)
        {
            ResetAnalysisOutput();

            StatusText =
                "Análisis cancelado";

            StatusDetail =
                "El archivo no fue modificado y puedes iniciar el análisis nuevamente.";

            ProgressPercentage = 0;

            ProgressLinesText =
                "Análisis cancelado";

            ProgressBytesText =
                string.Empty;
        }
        catch (Exception exception)
        {
            SetError(
                "No se pudo completar el análisis",
                exception.Message);
        }
        finally
        {
            IsAnalyzing = false;

            if (
                ReferenceEquals(
                    _analysisCancellation,
                    cancellationSource))
            {
                _analysisCancellation = null;
            }

            cancellationSource.Dispose();

            RefreshCommands();
        }
    }

    private void ReportProgress(
        LogReadProgress progress)
    {
        ProgressPercentage =
            progress.Percentage;

        ProgressLinesText =
            $"{progress.LinesRead:N0} líneas leídas";

        ProgressBytesText =
            progress.TotalBytes == 0
                ? FormatBytes(progress.BytesRead)
                : $"{FormatBytes(progress.BytesRead)} de " +
                  $"{FormatBytes(progress.TotalBytes)}";

        StatusDetail =
            progress.IsCompleted
                ? "Lectura completada. Generando diagnósticos."
                : $"Procesando {progress.Percentage:0.##}% del archivo.";
    }

    private void ApplyResult(
        LogFileDiagnosticResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        GroupItems =
            result.Groups
                .OrderByDescending(
                    group =>
                        GetLogLevelWeight(
                            group.HighestLevel))
                .ThenByDescending(
                    group =>
                        group.OccurrenceCount)
                .ThenBy(
                    group =>
                        group.RepresentativeMessage,
                    StringComparer.OrdinalIgnoreCase)
                .Select(
                    group =>
                        new IncidentGroupItemViewModel(
                            group))
                .ToArray();

        DiagnosisItems =
            result.Diagnoses
                .Select(
                    diagnosis =>
                        new DiagnosisItemViewModel(
                            diagnosis))
                .ToArray();

        SelectedGroup =
            GroupItems.FirstOrDefault();

        SelectedDiagnosis =
            DiagnosisItems.FirstOrDefault();

        TotalLinesText =
            result.TotalLines.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        ParsedLinesText =
            result.ParsedLines.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        GroupCountText =
            result.GroupCount.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        DiagnosisCountText =
            result.DiagnosisCount.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        ImmediateAttentionText =
            result.ImmediateAttentionCount.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        CriticalStatusText =
            result.HasCriticalIncidents
                ? "Sí"
                : "No";

        AnalysisCompletedText =
            result.CompletedAt.ToString(
                "yyyy-MM-dd HH:mm:ss zzz",
                CultureInfo.InvariantCulture);

        ProgressPercentage = 100;

        ProgressLinesText =
            $"{result.TotalLines:N0} líneas leídas";

        ProgressBytesText =
            "Análisis completo";

        if (result.TotalLines == 0)
        {
            HasResults = false;
            IsEmptyResult = true;

            StatusText =
                "El archivo está vacío";

            StatusDetail =
                "Selecciona un archivo que contenga entradas de logs.";

            return;
        }

        HasResults =
            result.GroupCount > 0 ||
            result.DiagnosisCount > 0;

        IsEmptyResult =
            !HasResults;

        if (IsEmptyResult)
        {
            StatusText =
                "No se encontraron resultados";

            StatusDetail =
                "El archivo contiene líneas, pero ninguna pudo agruparse o diagnosticarse.";

            return;
        }

        StatusText =
            result.HasCriticalIncidents
                ? "Análisis terminado con incidentes críticos"
                : "Análisis terminado";

        StatusDetail =
            $"{result.GroupCount:N0} grupos y " +
            $"{result.DiagnosisCount:N0} diagnósticos detectados.";
    }

    private void CancelAnalysis()
    {
        if (!IsAnalyzing)
        {
            return;
        }

        StatusText =
            "Cancelando análisis";

        StatusDetail =
            "Esperando a que finalice la operación actual.";

        _analysisCancellation?.Cancel();
    }

    private void Clear()
    {
        _analysisCancellation?.Cancel();

        SelectedFilePath = null;

        SelectedFileName =
            "Ningún archivo seleccionado";

        SelectedFileMetadata =
            "Formatos: .log, .txt, .jsonl y .ndjson";

        ResetAnalysisOutput();

        IsError = false;
        IsEmptyResult = false;

        StatusText =
            "Selecciona un archivo de logs";

        StatusDetail =
            "También puedes arrastrar un archivo hasta la ventana.";

        RefreshCommands();
    }

    private void ResetAnalysisOutput()
    {
        GroupItems = [];
        DiagnosisItems = [];

        SelectedGroup = null;
        SelectedDiagnosis = null;

        HasResults = false;
        IsError = false;
        IsEmptyResult = false;

        ProgressPercentage = 0;

        ProgressLinesText =
            "0 líneas leídas";

        ProgressBytesText =
            "0 B";

        TotalLinesText = "0";
        ParsedLinesText = "0";
        GroupCountText = "0";
        DiagnosisCountText = "0";
        ImmediateAttentionText = "0";
        CriticalStatusText = "No";

        AnalysisCompletedText =
            "No analizado";
    }

    private void SetError(
        string title,
        string detail)
    {
        ResetAnalysisOutput();

        IsError = true;
        IsEmptyResult = false;

        StatusText = title;

        StatusDetail =
            string.IsNullOrWhiteSpace(detail)
                ? "Ocurrió un error desconocido."
                : detail.Trim();

        RefreshCommands();
    }

    private void RaiseDisplayStateProperties()
    {
        RaisePropertyChanged(
            nameof(ShowStartState));

        RaisePropertyChanged(
            nameof(ShowProgressState));

        RaisePropertyChanged(
            nameof(ShowResultsState));

        RaisePropertyChanged(
            nameof(ShowErrorState));

        RaisePropertyChanged(
            nameof(ShowEmptyState));
    }

    private void RefreshCommands()
    {
        SelectFileCommand
            .NotifyCanExecuteChanged();

        AnalyzeCommand
            .NotifyCanExecuteChanged();

        CancelCommand
            .NotifyCanExecuteChanged();

        ClearCommand
            .NotifyCanExecuteChanged();
    }

    private static int GetLogLevelWeight(
        LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => 6,
            LogLevel.Error => 5,
            LogLevel.Warning => 4,
            LogLevel.Information => 3,
            LogLevel.Debug => 2,
            LogLevel.Trace => 1,
            _ => 0
        };
    }

    private static string FormatBytes(
        long bytes)
    {
        string[] units =
        [
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        ];

        double size =
            Math.Max(0, bytes);

        int unitIndex = 0;

        while (
            size >= 1024 &&
            unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return
            $"{size:0.##} {units[unitIndex]}";
    }

    public void Dispose()
    {
        _analysisCancellation?.Cancel();
        _analysisCancellation?.Dispose();
        _analysisCancellation = null;
    }
}