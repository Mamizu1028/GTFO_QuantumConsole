using System;
using UnityEngine;

namespace Hikaria.QC.Utilities;

public static class LogLevelExtensions
{
    /// <summary>
    ///     Gets the highest log level when there could potentially be multiple levels provided.
    /// </summary>
    /// <param name="levels">The log level(s).</param>
    /// <returns>The highest log level supplied.</returns>
    public static LogLevel GetHighestLevel(this LogLevel levels)
    {
        Array values = Enum.GetValues(typeof(LogLevel));
        Array.Sort(values);
        foreach (object obj in values)
        {
            LogLevel e = (LogLevel)obj;
            if ((levels & e) != LogLevel.None)
            {
                return e;
            }
        }
        return LogLevel.None;
    }

    /// <summary>
    ///     Returns a translation of a log level to it's associated console colour.
    /// </summary>
    /// <param name="level">The log level(s).</param>
    /// <returns>A console color associated with the highest log level supplied.</returns>
    public static Color GetUnityColorFromTheme(this LogLevel level, QuantumTheme theme)
    {
        level = level.GetHighestLevel();
        switch (level)
        {
            case LogLevel.Fatal:
                return theme.FatalErrorColor;
            case LogLevel.Error:
                return theme.ErrorColor;
            case LogLevel.Warning:
                return theme.WarningColor;
            case LogLevel.Message:
                return theme.MessageColor;
            case LogLevel.Info:
                return theme.InfoColor;
            case LogLevel.Debug:
                return theme.DebugColor;
            default:
                break;
        }
        return ColorExtensions.BRIGHT_GRAY;
    }
}
