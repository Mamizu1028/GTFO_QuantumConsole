namespace Hikaria.QC;

public readonly struct LogOperation
{
    public LogOperationKind Kind { get; }
    public LogHandle Id { get; }
    public string Text { get; }
    public LogLevel Level { get; }
    public bool HasFinalText { get; }
    public LogClearScope ClearScope { get; }

    private LogOperation(LogOperationKind kind, LogHandle id, string text,
        LogLevel level, bool hasFinalText, LogClearScope clearScope)
    {
        Kind = kind;
        Id = id;
        Text = text ?? string.Empty;
        Level = level;
        HasFinalText = hasFinalText;
        ClearScope = clearScope;
    }

    public static LogOperation AddHistory(LogHandle id, string text, LogLevel level)
    {
        return new LogOperation(LogOperationKind.AddHistory, id, text, level, false, LogClearScope.None);
    }

    public static LogOperation BeginHistoryStream(LogHandle id, string text, LogLevel level)
    {
        return new LogOperation(LogOperationKind.BeginHistoryStream, id, text, level, false, LogClearScope.None);
    }

    public static LogOperation AddLiveStatus(LogHandle id, string text, LogLevel level)
    {
        return new LogOperation(LogOperationKind.AddLiveStatus, id, text, level, false, LogClearScope.None);
    }

    public static LogOperation AddInteractive(LogHandle id, string text, LogLevel level)
    {
        return new LogOperation(LogOperationKind.AddInteractive, id, text, level, false, LogClearScope.None);
    }

    public static LogOperation Update(LogHandle id, string text, LogLevel level)
    {
        return new LogOperation(LogOperationKind.Update, id, text, level, false, LogClearScope.None);
    }

    public static LogOperation Append(LogHandle id, string text)
    {
        return new LogOperation(LogOperationKind.Append, id, text, LogLevel.Message, false, LogClearScope.None);
    }

    public static LogOperation Complete(LogHandle id)
    {
        return new LogOperation(LogOperationKind.Complete, id, string.Empty,
            LogLevel.Message, false, LogClearScope.None);
    }

    public static LogOperation Commit(LogHandle id, string? finalText, LogLevel level)
    {
        return new LogOperation(LogOperationKind.Commit, id, finalText ?? string.Empty,
            level, finalText != null, LogClearScope.None);
    }

    public static LogOperation Remove(LogHandle id)
    {
        return new LogOperation(LogOperationKind.Remove, id, string.Empty, LogLevel.Message, false, LogClearScope.None);
    }

    public static LogOperation Clear(LogClearScope scope)
    {
        return new LogOperation(LogOperationKind.Clear, LogHandle.Invalid, string.Empty,
            LogLevel.Message, false, scope);
    }
}
