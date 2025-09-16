namespace Hikaria.QC;

public class QuantumConsolePreferences
{
    public bool VerboseErrors = false;
    public LogLevel VerboseLogging = LogLevel.None;
    public LogLevel LoggingLevel = LogLevel.All;

    public LogLevel OpenOnLogLevel = LogLevel.None;
    public bool InterceptDebugLogger = true;
    public bool InterceptWhilstInactive = true;
    public bool PrependTimestamps = true;

    public bool ActivateOnStartup = false;
    public bool InitialiseOnStartup = true;
    public bool FocusOnActivate = true;
    public bool CloseOnSubmit = false;
    public AutoScrollOptions AutoScroll = AutoScrollOptions.OnInvoke;

    public bool EnableAutocomplete = true;
    public bool ShowPopupDisplay = true;
    public SortOrder SuggestionDisplayOrder = SortOrder.Descending;
    public int MaxSuggestionDisplaySize = 20;
    public bool UseFuzzySearch = true;
    public bool CaseSensitiveSearch = false;
    public bool CollapseSuggestionOverloads = true;

    public bool ShowCurrentJobs = true;
    public bool BlockOnAsync = false;

    public bool StoreCommandHistory = true;
    public bool StoreDuplicateCommands = true;
    public bool StoreAdjacentDuplicateCommands = false;
    public int CommandHistorySize = 30;

    public int MaxStoredLogs = 1024;
    public bool ShowInitLogs = true;

    public int LogFontSize = 14;
    public int SuggestionFontSize = 16;
}

