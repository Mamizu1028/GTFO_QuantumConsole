using UnityEngine;

namespace Hikaria.QC
{
    public static class LogTypeExtensions
    {
        public static LogLevel ToLogLevel(this LogType logType)
        {
            LogLevel level = LogLevel.None;
            switch (logType)
            {
                case LogType.Exception: level = LogLevel.Fatal; break;
                case LogType.Error: level = LogLevel.Error; break;
                case LogType.Assert: level = LogLevel.Error; break;
                case LogType.Warning: level = LogLevel.Warning; break;
                case LogType.Log: level = LogLevel.Debug; break;
            }

            return level;
        }
    }
}
