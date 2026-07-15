namespace Hikaria.QC
{
    public interface ILogController
    {
        int MaxHistoryLogs { get; set; }

        bool IsDirty { get; }

        void EnqueueOperation(LogOperation operation);
        void Clear(LogClearScope scope);

        void ProcessStorage();
        void ProcessLogs();

        void RebuildLogTextLayout(bool viewportSizeChanged);

        void ScrollToLatest(bool immediate);
    }
}
