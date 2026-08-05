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

    private readonly IClipboardService
        _clipboardService;

    private readonly LogAnalysisExplorerService
        _explorerService;

    private readonly StartupSummary
        _startupSummary;

    private readonly IReadOnlyList<LogLevelFilterOption>
        _levelFilters;

    private readonly IReadOnlyList<PriorityFilterOption>
        _priorityFilters;

    private readonly IReadOnlyList<GroupSortOption>
        _groupSortOptions;

    private readonly IReadOnlyList<DiagnosisSortOption>
        _diagnosisSortOptions;

    private IReadOnlyList<LogGroupSummary>
        _allGroups =
            [];

    private IReadOnlyList<IncidentDiagnosis>
        _allDiagnoses =
            [];

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

    private string _searchText =
        string.Empty;

    private string _filterSummaryText =
        "Sin resultados cargados";

    private string _copyStatusText =
        string.Empty;

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

    private string _visibleGroupCountText =
        "0";

    private string _visibleDiagnosisCountText =
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

    private LogSampleItemViewModel?
        _selectedSample;

    private LogLevelFilterOption
        _selectedLevelFilter;

    private PriorityFilterOption
        _selectedPriorityFilter;

    private GroupSortOption
        _selectedGroupSort;

    private DiagnosisSortOption
        _selectedDiagnosisSort;

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
        "Fase 8 · Exploración, filtros y copia";

    public string RuntimeDescription =>
        $"{_startupSummary.Runtime.OperatingSystem} · " +
        $"{_startupSummary.Runtime.Architecture} · " +
        $"{_startupSummary.Runtime.Framework}";

    public IReadOnlyList<LogLevelFilterOption>
        LevelFilters =>
            _levelFilters;

    public IReadOnlyList<PriorityFilterOption>
        PriorityFilters =>
            _priorityFilters;

    public IReadOnlyList<GroupSortOption>
        GroupSortOptions =>
            _groupSortOptions;

    public IReadOnlyList<DiagnosisSortOption>
        DiagnosisSortOptions =>
            _diagnosisSortOptions;

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

    public string SearchText
    {
        get => _searchText;

        set
        {
            if (
                SetProperty(
                    ref _searchText,
                    value ?? string.Empty))
            {
                ApplyFilters();
            }
        }
    }

    public string FilterSummaryText
    {
        get => _filterSummaryText;

        private set =>
            SetProperty(
                ref _filterSummaryText,
                value);
    }

    public string CopyStatusText
    {
        get => _copyStatusText;

        private set
        {
            if (
                SetProperty(
                    ref _copyStatusText,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasCopyStatus));
            }
        }
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

    public string VisibleGroupCountText
    {
        get => _visibleGroupCountText;

        private set =>
            SetProperty(
                ref _visibleGroupCountText,
                value);
    }

    public string VisibleDiagnosisCountText
    {
        get => _visibleDiagnosisCountText;

        private set =>
            SetProperty(
                ref _visibleDiagnosisCountText,
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
                SelectedSample =
                    value?.Samples.FirstOrDefault();

                RaisePropertyChanged(
                    nameof(HasSelectedGroup));

                RaisePropertyChanged(
                    nameof(HasNoSelectedGroup));

                RefreshCommands();
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

                RefreshCommands();
            }
        }
    }

    public LogSampleItemViewModel?
        SelectedSample
    {
        get => _selectedSample;

        set
        {
            if (
                SetProperty(
                    ref _selectedSample,
                    value))
            {
                RaisePropertyChanged(
                    nameof(HasSelectedSample));

                RefreshCommands();
            }
        }
    }

    public LogLevelFilterOption
        SelectedLevelFilter
    {
        get => _selectedLevelFilter;

        set
        {
            if (
                value is not null &&
                SetProperty(
                    ref _selectedLevelFilter,
                    value))
            {
                ApplyFilters();
            }
        }
    }

    public PriorityFilterOption
        SelectedPriorityFilter
    {
        get => _selectedPriorityFilter;

        set
        {
            if (
                value is not null &&
                SetProperty(
                    ref _selectedPriorityFilter,
                    value))
            {
                ApplyFilters();
            }
        }
    }

    public GroupSortOption
        SelectedGroupSort
    {
        get => _selectedGroupSort;

        set
        {
            if (
                value is not null &&
                SetProperty(
                    ref _selectedGroupSort,
                    value))
            {
                ApplyFilters();
            }
        }
    }

    public DiagnosisSortOption
        SelectedDiagnosisSort
    {
        get => _selectedDiagnosisSort;

        set
        {
            if (
                value is not null &&
                SetProperty(
                    ref _selectedDiagnosisSort,
                    value))
            {
                ApplyFilters();
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

    public bool HasSelectedSample =>
        SelectedSample is not null;

    public bool HasCopyStatus =>
        !string.IsNullOrWhiteSpace(
            CopyStatusText);

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(
            SearchText) ||
        SelectedLevelFilter.Value.HasValue ||
        SelectedPriorityFilter.Value.HasValue ||
        SelectedGroupSort.Value !=
            LogGroupSortOrder.Severity ||
        SelectedDiagnosisSort.Value !=
            IncidentDiagnosisSortOrder.Priority;

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

    public RelayCommand
        ResetFiltersCommand { get; }

    public AsyncRelayCommand
        CopyGroupMessageCommand { get; }

    public AsyncRelayCommand
        CopyGroupFingerprintCommand { get; }

    public AsyncRelayCommand
        CopyGroupDetailsCommand { get; }

    public AsyncRelayCommand
        CopySampleCommand { get; }

    public AsyncRelayCommand
        CopyDiagnosisSummaryCommand { get; }

    public AsyncRelayCommand
        CopyDiagnosisFingerprintCommand { get; }

    public AsyncRelayCommand
        CopyDiagnosisActionsCommand { get; }

    public AsyncRelayCommand
        CopyDiagnosisDetailsCommand { get; }

    public MainWindowViewModel(
        ILogFilePickerService filePickerService,
        IDesktopLogAnalysisService analysisService,
        IClipboardService clipboardService,
        LogAnalysisExplorerService explorerService)
    {
        ArgumentNullException.ThrowIfNull(
            filePickerService);

        ArgumentNullException.ThrowIfNull(
            analysisService);

        ArgumentNullException.ThrowIfNull(
            clipboardService);

        ArgumentNullException.ThrowIfNull(
            explorerService);

        _filePickerService =
            filePickerService;

        _analysisService =
            analysisService;

        _clipboardService =
            clipboardService;

        _explorerService =
            explorerService;

        StartupSummaryService startupService = new(
            new RuntimeEnvironmentProvider());

        _startupSummary =
            startupService.Create();

        _levelFilters =
        [
            new(
                "Todos los niveles",
                null),

            new(
                "Critical",
                LogLevel.Critical),

            new(
                "Error",
                LogLevel.Error),

            new(
                "Warning",
                LogLevel.Warning),

            new(
                "Information",
                LogLevel.Information),

            new(
                "Debug",
                LogLevel.Debug),

            new(
                "Trace",
                LogLevel.Trace),

            new(
                "Unknown",
                LogLevel.Unknown)
        ];

        _priorityFilters =
        [
            new(
                "Todas las prioridades",
                null),

            new(
                "Crítica",
                IncidentPriority.Critical),

            new(
                "Alta",
                IncidentPriority.High),

            new(
                "Media",
                IncidentPriority.Medium),

            new(
                "Baja",
                IncidentPriority.Low),

            new(
                "Sin prioridad",
                IncidentPriority.None)
        ];

        _groupSortOptions =
        [
            new(
                "Gravedad",
                LogGroupSortOrder.Severity),

            new(
                "Frecuencia",
                LogGroupSortOrder.Frequency),

            new(
                "Más recientes",
                LogGroupSortOrder.Newest),

            new(
                "Más antiguos",
                LogGroupSortOrder.Oldest),

            new(
                "Mensaje",
                LogGroupSortOrder.Message)
        ];

        _diagnosisSortOptions =
        [
            new(
                "Prioridad",
                IncidentDiagnosisSortOrder.Priority),

            new(
                "Confianza",
                IncidentDiagnosisSortOrder.Confidence),

            new(
                "Más recientes",
                IncidentDiagnosisSortOrder.Newest),

            new(
                "Título",
                IncidentDiagnosisSortOrder.Title)
        ];

        _selectedLevelFilter =
            _levelFilters[0];

        _selectedPriorityFilter =
            _priorityFilters[0];

        _selectedGroupSort =
            _groupSortOptions[0];

        _selectedDiagnosisSort =
            _diagnosisSortOptions[0];

        SelectFileCommand =
            new AsyncRelayCommand(
                SelectFileAsync,
                () => !IsAnalyzing);

        AnalyzeCommand =
            new AsyncRelayCommand(
                AnalyzeSelectedFileAsync,
                () =>
                    HasSelectedFile &&
                    !IsAnalyzing);

        CancelCommand =
            new RelayCommand(
                CancelAnalysis,
                () => IsAnalyzing);

        ClearCommand =
            new RelayCommand(
                Clear,
                () =>
                    !IsAnalyzing &&
                    (
                        HasSelectedFile ||
                        HasResults ||
                        IsError ||
                        IsEmptyResult
                    ));

        ResetFiltersCommand =
            new RelayCommand(
                ResetFilters,
                () =>
                    HasResults &&
                    HasActiveFilters);

        CopyGroupMessageCommand =
            new AsyncRelayCommand(
                CopyGroupMessageAsync,
                () =>
                    SelectedGroup is not null);

        CopyGroupFingerprintCommand =
            new AsyncRelayCommand(
                CopyGroupFingerprintAsync,
                () =>
                    SelectedGroup is not null);

        CopyGroupDetailsCommand =
            new AsyncRelayCommand(
                CopyGroupDetailsAsync,
                () =>
                    SelectedGroup is not null);

        CopySampleCommand =
            new AsyncRelayCommand(
                CopySampleAsync,
                () =>
                    SelectedSample is not null);

        CopyDiagnosisSummaryCommand =
            new AsyncRelayCommand(
                CopyDiagnosisSummaryAsync,
                () =>
                    SelectedDiagnosis is not null);

        CopyDiagnosisFingerprintCommand =
            new AsyncRelayCommand(
                CopyDiagnosisFingerprintAsync,
                () =>
                    SelectedDiagnosis is not null);

        CopyDiagnosisActionsCommand =
            new AsyncRelayCommand(
                CopyDiagnosisActionsAsync,
                () =>
                    SelectedDiagnosis is not null);

        CopyDiagnosisDetailsCommand =
            new AsyncRelayCommand(
                CopyDiagnosisDetailsAsync,
                () =>
                    SelectedDiagnosis is not null);
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

            if (
                string.IsNullOrWhiteSpace(
                    filePath))
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
                !SupportedLogFileExtensions
                    .IsSupported(fullPath))
            {
                throw new NotSupportedException(
                    "LogLens admite archivos .log, .txt, .jsonl y .ndjson.");
            }

            FileInfo fileInfo =
                new(fullPath);

            ResetAnalysisOutput();
            ResetFilters();

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

        if (
            string.IsNullOrWhiteSpace(
                filePath))
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

        Progress<LogReadProgress> progress =
            new(ReportProgress);

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
                ? FormatBytes(
                    progress.BytesRead)
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

        _allGroups =
            result.Groups.ToArray();

        _allDiagnoses =
            result.Diagnoses.ToArray();

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

        ApplyFilters();

        StatusText =
            result.HasCriticalIncidents
                ? "Análisis terminado con incidentes críticos"
                : "Análisis terminado";

        StatusDetail =
            $"{result.GroupCount:N0} grupos y " +
            $"{result.DiagnosisCount:N0} diagnósticos detectados.";
    }

    private void ApplyFilters()
    {
        string? selectedGroupFingerprint =
            SelectedGroup?.Fingerprint;

        string? selectedDiagnosisKey =
            SelectedDiagnosis is null
                ? null
                : $"{SelectedDiagnosis.RuleId}|" +
                  $"{SelectedDiagnosis.Fingerprint}";

        IReadOnlyList<LogGroupSummary> filteredGroups =
            _explorerService.QueryGroups(
                _allGroups,
                new LogGroupQueryOptions(
                    SearchText,
                    SelectedLevelFilter.Value,
                    SelectedGroupSort.Value));

        IReadOnlyList<IncidentDiagnosis>
            filteredDiagnoses =
                _explorerService.QueryDiagnoses(
                    _allDiagnoses,
                    new IncidentDiagnosisQueryOptions(
                        SearchText,
                        SelectedPriorityFilter.Value,
                        SelectedDiagnosisSort.Value));

        GroupItems =
            filteredGroups
                .Select(
                    group =>
                        new IncidentGroupItemViewModel(
                            group))
                .ToArray();

        DiagnosisItems =
            filteredDiagnoses
                .Select(
                    diagnosis =>
                        new DiagnosisItemViewModel(
                            diagnosis))
                .ToArray();

        SelectedGroup =
            GroupItems.FirstOrDefault(
                group =>
                    string.Equals(
                        group.Fingerprint,
                        selectedGroupFingerprint,
                        StringComparison.Ordinal))
            ?? GroupItems.FirstOrDefault();

        SelectedDiagnosis =
            DiagnosisItems.FirstOrDefault(
                diagnosis =>
                    string.Equals(
                        $"{diagnosis.RuleId}|" +
                        $"{diagnosis.Fingerprint}",
                        selectedDiagnosisKey,
                        StringComparison.Ordinal))
            ?? DiagnosisItems.FirstOrDefault();

        VisibleGroupCountText =
            GroupItems.Count.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        VisibleDiagnosisCountText =
            DiagnosisItems.Count.ToString(
                "N0",
                CultureInfo.InvariantCulture);

        FilterSummaryText =
            $"{GroupItems.Count:N0} de " +
            $"{_allGroups.Count:N0} grupos · " +
            $"{DiagnosisItems.Count:N0} de " +
            $"{_allDiagnoses.Count:N0} diagnósticos";

        CopyStatusText =
            string.Empty;

        RaisePropertyChanged(
            nameof(HasActiveFilters));

        RefreshCommands();
    }

    private void ResetFilters()
    {
        _searchText =
            string.Empty;

        _selectedLevelFilter =
            _levelFilters[0];

        _selectedPriorityFilter =
            _priorityFilters[0];

        _selectedGroupSort =
            _groupSortOptions[0];

        _selectedDiagnosisSort =
            _diagnosisSortOptions[0];

        RaisePropertyChanged(
            nameof(SearchText));

        RaisePropertyChanged(
            nameof(SelectedLevelFilter));

        RaisePropertyChanged(
            nameof(SelectedPriorityFilter));

        RaisePropertyChanged(
            nameof(SelectedGroupSort));

        RaisePropertyChanged(
            nameof(SelectedDiagnosisSort));

        RaisePropertyChanged(
            nameof(HasActiveFilters));

        ApplyFilters();
    }

    private async Task CopyGroupMessageAsync()
    {
        IncidentGroupItemViewModel? group =
            SelectedGroup;

        if (group is null)
        {
            return;
        }

        await CopyTextAsync(
            group.Message,
            "Mensaje del incidente copiado.");
    }

    private async Task CopyGroupFingerprintAsync()
    {
        IncidentGroupItemViewModel? group =
            SelectedGroup;

        if (group is null)
        {
            return;
        }

        await CopyTextAsync(
            group.Fingerprint,
            "Huella del incidente copiada.");
    }

    private async Task CopyGroupDetailsAsync()
    {
        IncidentGroupItemViewModel? group =
            SelectedGroup;

        if (group is null)
        {
            return;
        }

        await CopyTextAsync(
            group.CompleteText,
            "Detalles del incidente copiados.");
    }

    private async Task CopySampleAsync()
    {
        LogSampleItemViewModel? sample =
            SelectedSample;

        if (sample is null)
        {
            return;
        }

        await CopyTextAsync(
            sample.CompleteText,
            "Muestra del log copiada.");
    }

    private async Task CopyDiagnosisSummaryAsync()
    {
        DiagnosisItemViewModel? diagnosis =
            SelectedDiagnosis;

        if (diagnosis is null)
        {
            return;
        }

        await CopyTextAsync(
            diagnosis.Summary,
            "Resumen del diagnóstico copiado.");
    }

    private async Task CopyDiagnosisFingerprintAsync()
    {
        DiagnosisItemViewModel? diagnosis =
            SelectedDiagnosis;

        if (diagnosis is null)
        {
            return;
        }

        await CopyTextAsync(
            diagnosis.Fingerprint,
            "Huella del diagnóstico copiada.");
    }

    private async Task CopyDiagnosisActionsAsync()
    {
        DiagnosisItemViewModel? diagnosis =
            SelectedDiagnosis;

        if (diagnosis is null)
        {
            return;
        }

        await CopyTextAsync(
            diagnosis.ActionsText,
            "Acciones recomendadas copiadas.");
    }

    private async Task CopyDiagnosisDetailsAsync()
    {
        DiagnosisItemViewModel? diagnosis =
            SelectedDiagnosis;

        if (diagnosis is null)
        {
            return;
        }

        await CopyTextAsync(
            diagnosis.CompleteText,
            "Diagnóstico completo copiado.");
    }

    private async Task CopyTextAsync(
        string text,
        string successMessage)
    {
        try
        {
            await _clipboardService.SetTextAsync(
                text);

            CopyStatusText =
                successMessage;
        }
        catch (Exception exception)
        {
            CopyStatusText =
                $"No se pudo copiar: " +
                $"{exception.Message}";
        }
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
        ResetFilters();

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
        _allGroups = [];
        _allDiagnoses = [];

        GroupItems = [];
        DiagnosisItems = [];

        SelectedGroup = null;
        SelectedDiagnosis = null;
        SelectedSample = null;

        HasResults = false;
        IsError = false;
        IsEmptyResult = false;

        CopyStatusText =
            string.Empty;

        FilterSummaryText =
            "Sin resultados cargados";

        ProgressPercentage = 0;

        ProgressLinesText =
            "0 líneas leídas";

        ProgressBytesText =
            "0 B";

        TotalLinesText = "0";
        ParsedLinesText = "0";
        GroupCountText = "0";
        DiagnosisCountText = "0";
        VisibleGroupCountText = "0";
        VisibleDiagnosisCountText = "0";
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

        ResetFiltersCommand
            .NotifyCanExecuteChanged();

        CopyGroupMessageCommand
            .NotifyCanExecuteChanged();

        CopyGroupFingerprintCommand
            .NotifyCanExecuteChanged();

        CopyGroupDetailsCommand
            .NotifyCanExecuteChanged();

        CopySampleCommand
            .NotifyCanExecuteChanged();

        CopyDiagnosisSummaryCommand
            .NotifyCanExecuteChanged();

        CopyDiagnosisFingerprintCommand
            .NotifyCanExecuteChanged();

        CopyDiagnosisActionsCommand
            .NotifyCanExecuteChanged();

        CopyDiagnosisDetailsCommand
            .NotifyCanExecuteChanged();
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
            Math.Max(
                0,
                bytes);

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