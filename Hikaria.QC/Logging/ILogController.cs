using System.Collections.Generic;

namespace Hikaria.QC
{
    public interface ILogController
    {
        int MaxStoredLogs { get; set; }

        IReadOnlyList<ILogData> LogDatas { get; }

        bool IsDirty { get; }

        void AddLog(ILog log);
        void RemoveLog();
        void Clear();

        void ProcessLogs();

        void RebuildLogTextLayout(bool viewportSizeChanged);

        void ScrollToLatest(bool immediate);
    }
}