using System;
using System.Collections.Generic;
using System.Text;

namespace Hikaria.QC;

public class LogCellData : ILogData
{
    private readonly StringBuilder _logTraceBuilder;
    private readonly List<ILog> _logs;
    private string _cachedString;
    private bool _stringDirty;

    private float _cellSize;

    public bool IsDirty { get; set; } = true;

    public float CellSize
    {
        get => _cellSize;
        set
        {
            if (Math.Abs(_cellSize - value) > 0.01f)
            {
                _cellSize = value;
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ILog> Logs => _logs;

    public override string ToString() => GetLogString();

    public string GetLogString()
    {
        if (_stringDirty || _cachedString == null)
        {
            _cachedString = _logTraceBuilder.ToString();
            _stringDirty = false;
        }
        return _cachedString;
    }

    public LogCellData()
    {
        _logs = new List<ILog>();
        _logTraceBuilder = new StringBuilder();
        _cachedString = string.Empty;
        _stringDirty = true;
    }

    public LogCellData(ILog log) : this()
    {
        Reset(log);
    }

    public void Reset(ILog log)
    {
        _logs.Clear();
        _logTraceBuilder.Clear();

        _logs.Add(log);
        _logTraceBuilder.Append(log.Text);

        _stringDirty = true;
        IsDirty = true;
        _cellSize = 0f;
    }

    public void AppendLog(ILog log)
    {
        _logTraceBuilder.Append(log.Text);
        _logs.Add(log);

        _stringDirty = true;
        IsDirty = true;
    }

    public bool RemoveLog()
    {
        if (_logs.Count > 0)
        {
            var log = _logs[^1];
            _logs.RemoveAt(_logs.Count - 1);
            _logTraceBuilder.Remove(_logTraceBuilder.Length - log.Text.Length, log.Text.Length);

            _stringDirty = true;
            IsDirty = true;
            return true;
        }

        return false;
    }

    public void Clear()
    {
        _logs.Clear();
        _logTraceBuilder.Clear();

        _stringDirty = true;
        IsDirty = true;
    }
}
