using System.Collections.Generic;
using System.Text;

namespace Hikaria.QC;

public class LogCellData : ILogData
{
    private readonly StringBuilder _logTraceBuilder;
    private readonly List<ILog> _logs;

    public bool IsDirty { get; set; } = true;

    public float CellSize
    {
        get => _cellSize;
        set
        {
            if (_cellSize != value)
            {
                _cellSize = value;
                IsDirty = true;
            }
        }
    }
    private float _cellSize = 0f;

    public IReadOnlyList<ILog> Logs => _logs;

    public override string ToString()
    {
        return _logTraceBuilder.ToString();
    }

    public string GetLogString()
    {
        return _logTraceBuilder.ToString();
    }

    public LogCellData(ILog log)
    {
        _logs = new List<ILog>{ log };
        _logTraceBuilder = new StringBuilder(log.Text);
    }

    public void AppendLog(ILog log)
    {
        int logLength = _logTraceBuilder.Length + log.Text.Length;
        int capacity = _logTraceBuilder.Capacity;
        while (capacity < logLength)
        {
            capacity *= 2;
        }
        _logTraceBuilder.EnsureCapacity(capacity);
        _logTraceBuilder.Append(log.Text);

        _logs.Add(log);

        IsDirty = true;
    }

    public bool RemoveLog()
    {
        if (_logs.Count > 0)
        {
            var log = _logs[^1];
            _logs.RemoveAt(_logs.Count - 1);
            _logTraceBuilder.Remove(_logTraceBuilder.Length - log.Text.Length, log.Text.Length);
            IsDirty = true;
            return true;
        }

        return false;
    }

    public void Clear()
    {
        IsDirty = true;
        _logs.Clear();
        _logTraceBuilder.Clear();
    }
}
