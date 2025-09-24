using System.Collections.Generic;
using System.Text;

namespace Hikaria.QC;

public class LogCellData : ILogData
{
    private StringBuilder _logTraceBuilder;
    private List<ILog> _logs = new List<ILog>(5);

    public float CellSize { get; set; } = 0f;

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
        _logs.Add(log);
        _logTraceBuilder = new StringBuilder(log.Text);
    }

    public void AppendLog(ILog log)
    {
        _logs.Add(log);

        int logLength = _logTraceBuilder.Length + log.Text.Length;
        int capacity = _logTraceBuilder.Capacity;
        while (capacity < logLength)
        {
            capacity *= 2;
        }

        _logTraceBuilder.EnsureCapacity(capacity);
        _logTraceBuilder.Append(log.Text);
    }

    public bool RemoveLog()
    {
        if (_logs.Count > 0)
        {
            var log = _logs[_logs.Count - 1];
            _logs.RemoveAt(_logs.Count - 1);
            _logTraceBuilder.Remove(_logTraceBuilder.Length - log.Text.Length, log.Text.Length);
            return true;
        }

        return false;
    }

    public void Clear()
    {
        _logs.Clear();
        _logTraceBuilder.Clear();
    }
}
