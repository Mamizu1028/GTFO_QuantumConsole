using System.Collections.Generic;

namespace Hikaria.QC
{
    public interface ILogController
    {
        int MaxStoredLogs { get; set; }

        IReadOnlyList<ILogData> LogDatas { get; }

        void AddLog(ILog log);
        void RemoveLog();
        void Clear();

        void FlushLogText();

        void ScrollConsoleToLatest();
    }
}