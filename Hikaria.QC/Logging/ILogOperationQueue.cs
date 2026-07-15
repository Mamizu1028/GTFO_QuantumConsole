namespace Hikaria.QC;

public interface ILogOperationQueue
{
    bool IsEmpty { get; }

    void Enqueue(LogOperation operation);
    bool TryDequeue(out LogOperation operation);
    void Clear();
}
