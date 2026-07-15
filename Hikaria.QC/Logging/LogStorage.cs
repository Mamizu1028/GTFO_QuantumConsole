using System.Collections.Generic;

namespace Hikaria.QC;

internal sealed class LogStorage
{
    public const int DefaultMaxHistoryLogs = 4096;

    private enum LogCollection
    {
        History,
        Live
    }

    private readonly LogRingBuffer _historyLogs;
    private readonly List<Log> _liveLogs = new List<Log>();
    private readonly List<Log> _trimmedHistoryLogs = new List<Log>();
    private readonly Dictionary<LogHandle, LogRef> _handleLookup = new Dictionary<LogHandle, LogRef>();

    private int _maxHistoryLogs;
    private float _trimmedHistoryExtent;

    public LogStorage(int maxHistoryLogs = DefaultMaxHistoryLogs)
    {
        _maxHistoryLogs = NormalizeMaxHistoryLogs(maxHistoryLogs);
        _historyLogs = new LogRingBuffer(_maxHistoryLogs);
    }

    public int MaxHistoryLogs
    {
        get => _maxHistoryLogs;
        set
        {
            int maxHistoryLogs = NormalizeMaxHistoryLogs(value);
            if (_maxHistoryLogs == maxHistoryLogs)
                return;

            TrimHistoryToCapacity(maxHistoryLogs);
            _historyLogs.SetCapacity(maxHistoryLogs);
            _maxHistoryLogs = maxHistoryLogs;
        }
    }

    public IReadOnlyList<Log> HistoryLogs => _historyLogs;
    public IReadOnlyList<Log> LiveLogs => _liveLogs;
    internal IReadOnlyList<Log> TrimmedHistoryLogs => _trimmedHistoryLogs;
    internal float TrimmedHistoryExtent => _trimmedHistoryExtent;
    internal int HistoryCapacity => _historyLogs.Capacity;
    public int Count => _historyLogs.Count + _liveLogs.Count;

    public Log this[int index]
    {
        get
        {
            if (index < _historyLogs.Count)
                return _historyLogs[index];

            return _liveLogs[index - _historyLogs.Count];
        }
    }

    public bool TryGet(LogHandle id, out Log entry)
    {
        if (_handleLookup.TryGetValue(id, out LogRef logRef))
        {
            entry = logRef.Log;
            return true;
        }

        entry = null!;
        return false;
    }

    public bool Update(LogHandle id, string text, LogLevel level = LogLevel.Message)
    {
        if (!TryGet(id, out Log entry))
            return false;

        if (!entry.IsMutable)
            return false;

        entry.SetText(text, level);
        return true;
    }

    public bool Append(LogHandle id, string text)
    {
        if (!TryGet(id, out Log entry))
            return false;

        if (!entry.IsMutable)
            return false;

        entry.AppendText(text);
        return true;
    }

    public bool Complete(LogHandle id)
    {
        return Complete(id, out _);
    }

    public bool Complete(LogHandle id, out Log? affectedLog)
    {
        if (!TryGet(id, out Log entry))
        {
            affectedLog = null;
            return false;
        }

        if (!entry.IsMutable)
        {
            affectedLog = null;
            return false;
        }

        affectedLog = entry;

        if (entry.Kind == LogKind.HistoryStream)
        {
            entry.SetKind(LogKind.History);
            entry.CompactText();
            return true;
        }

        return Remove(id);
    }

    public bool Commit(LogHandle id, string? finalText = null, LogLevel level = LogLevel.Message)
    {
        return Commit(id, finalText, level, out _);
    }

    public bool Commit(LogHandle id, string? finalText, LogLevel level, out Log? affectedLog)
    {
        if (!_handleLookup.TryGetValue(id, out LogRef logRef))
        {
            affectedLog = null;
            return false;
        }

        Log entry = logRef.Log;
        if (!entry.IsMutable)
        {
            affectedLog = null;
            return false;
        }

        string text = finalText ?? entry.Text;
        affectedLog = entry;

        if (entry.Kind == LogKind.HistoryStream)
        {
            entry.SetText(text, level);
            entry.SetKind(LogKind.History);
            entry.CompactText();
            return true;
        }

        if (!MoveLiveToHistory(id, entry, text, level))
        {
            affectedLog = null;
            return false;
        }

        return true;
    }

    public bool Remove(LogHandle id)
    {
        if (!_handleLookup.TryGetValue(id, out LogRef logRef))
            return false;

        bool removed = logRef.Collection == LogCollection.History
            ? _historyLogs.Remove(logRef.Log)
            : _liveLogs.Remove(logRef.Log);

        if (!removed)
            return false;

        _handleLookup.Remove(id);
        return true;
    }

    public void Clear(LogClearScope scope = LogClearScope.History)
    {
        if (scope.HasFlag(LogClearScope.History))
            ClearCollection(LogCollection.History);

        if (scope.HasFlag(LogClearScope.Live))
            ClearCollection(LogCollection.Live);
    }

    internal bool AddHistory(LogHandle id, string text, LogLevel level)
    {
        return AddToHistoryLog(id, LogKind.History, text, level) != null;
    }

    internal bool BeginHistoryStream(LogHandle id, string text, LogLevel level)
    {
        if (!id.IsValid || _handleLookup.ContainsKey(id))
            return false;

        return AddToHistoryLog(id, LogKind.HistoryStream, text, level) != null;
    }

    internal bool AddLive(LogHandle id, string text, LogLevel level)
    {
        return AddToLive(id, LogKind.LiveStatus, text, level);
    }

    internal bool AddInteractive(LogHandle id, string text, LogLevel level)
    {
        return AddToLive(id, LogKind.Interactive, text, level);
    }

    internal void ClearTrimmedHistoryLogs()
    {
        _trimmedHistoryLogs.Clear();
        _trimmedHistoryExtent = 0f;
    }

    private Log? AddToHistoryLog(LogHandle id, LogKind kind, string text, LogLevel level)
    {
        if (!id.IsValid)
            return null;

        if (_maxHistoryLogs > 0)
            TrimHistoryToCapacity(_maxHistoryLogs - 1);

        var entry = new Log(id, kind, text, level);
        _historyLogs.AddLast(entry);
        _handleLookup.Add(id, new LogRef(LogCollection.History, entry));
        return entry;
    }

    private bool AddToLive(LogHandle id, LogKind kind, string text, LogLevel level)
    {
        if (!id.IsValid)
            return false;

        var entry = new Log(id, kind, text, level);
        _liveLogs.Add(entry);
        _handleLookup.Add(id, new LogRef(LogCollection.Live, entry));
        return true;
    }

    private bool MoveLiveToHistory(LogHandle id, Log entry, string text, LogLevel level)
    {
        if (!_liveLogs.Remove(entry))
            return false;

        if (_maxHistoryLogs > 0)
            TrimHistoryToCapacity(_maxHistoryLogs - 1);

        entry.SetText(text, level);
        entry.SetKind(LogKind.History);
        entry.CompactText();
        _historyLogs.AddLast(entry);
        _handleLookup[id] = new LogRef(LogCollection.History, entry);
        return true;
    }

    private void TrimHistoryToCapacity(int capacity)
    {
        while (_historyLogs.Count > capacity)
            RemoveOldestHistoryLog();
    }

    private void RemoveOldestHistoryLog()
    {
        Log dropped = _historyLogs.RemoveFirst();
        _handleLookup.Remove(dropped.Id);
        _trimmedHistoryLogs.Add(dropped);
        if (dropped.CellSize > 0f)
            _trimmedHistoryExtent += dropped.CellSize;
    }

    private static int NormalizeMaxHistoryLogs(int maxHistoryLogs)
    {
        return maxHistoryLogs > 0 ? maxHistoryLogs : DefaultMaxHistoryLogs;
    }

    private IEnumerable<Log> GetCollection(LogCollection collection)
    {
        return collection == LogCollection.History ? _historyLogs : _liveLogs;
    }

    private void ClearCollection(LogCollection collection)
    {
        foreach (Log log in GetCollection(collection))
            _handleLookup.Remove(log.Id);

        if (collection == LogCollection.History)
            _historyLogs.Clear();
        else
            _liveLogs.Clear();
    }

    private readonly struct LogRef
    {
        public readonly LogCollection Collection;
        public readonly Log Log;

        public LogRef(LogCollection collection, Log log)
        {
            Collection = collection;
            Log = log;
        }
    }
}
