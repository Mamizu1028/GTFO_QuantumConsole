using System.Collections.Concurrent;

namespace Hikaria.QC;

public class LogOperationQueue : ILogOperationQueue
{
    private readonly ConcurrentQueue<LogOperation> _queuedOperations = new ConcurrentQueue<LogOperation>();

    public bool IsEmpty => _queuedOperations.IsEmpty;

    public void Enqueue(LogOperation operation)
    {
        _queuedOperations.Enqueue(operation);
    }

    public bool TryDequeue(out LogOperation operation)
    {
        return _queuedOperations.TryDequeue(out operation);
    }

    public void Clear()
    {
        while (TryDequeue(out LogOperation _)) { }
    }
}
