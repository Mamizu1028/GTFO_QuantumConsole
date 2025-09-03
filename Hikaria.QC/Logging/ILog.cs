namespace Hikaria.QC
{
    public interface ILog
    {
        string Text { get; }
        LogLevel Level { get; }
        bool NewLine { get; }
    }
}