using System.Collections.Generic;

namespace Hikaria.QC
{
    public interface ILogData
    {
        IReadOnlyList<ILog> Logs { get; }

        string GetLogString();

        void AppendLog(ILog log);

        bool RemoveLog();

        void Clear();
    }
}
