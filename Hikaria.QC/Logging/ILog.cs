using UnityEngine;

namespace Hikaria.QC
{
    public interface ILog
    {
        string Text { get; }
        LogType Type { get; }
        bool NewLine { get; }
    }
}