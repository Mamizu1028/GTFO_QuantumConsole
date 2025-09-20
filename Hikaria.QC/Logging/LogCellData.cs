using System.Collections.Generic;
using System.Text;

namespace Hikaria.QC;

public class LogCellData : ILogData
{
    private StringBuilder LogTraceBuilder { get; }
    private List<ILog> _logs = new List<ILog>(10);

    public float CellSize { get; set; } = 0f;

    public IReadOnlyList<ILog> Logs => _logs;

    public override string ToString()
    {
        return LogTraceBuilder.ToString();
    }

    public string GetLogString()
    {
        return LogTraceBuilder.ToString();
    }

    public LogCellData(ILog log)
    {
        LogTraceBuilder = new StringBuilder(log.Text.Length);
        LogTraceBuilder.Append(log.Text);
    }

    private LogCellData() { }

    public void AppendLog(ILog log)
    {
        _logs.Add(log);

        int logLength = LogTraceBuilder.Length + log.Text.Length;
        int capacity = LogTraceBuilder.Capacity;
        while (capacity < logLength)
        {
            capacity *= 2;
        }

        LogTraceBuilder.EnsureCapacity(capacity);

        LogTraceBuilder.Append(log.Text);
    }

    public bool RemoveLog()
    {
        if (_logs.Count > 0)
        {
            _logs.RemoveAt(_logs.Count - 1);
            return true;
        }

        return false;
    }

    public void Clear()
    {
        _logs.Clear();
        LogTraceBuilder.Clear();
    }
}
