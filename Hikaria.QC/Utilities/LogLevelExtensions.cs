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
        if ((levels & LogLevel.Fatal) != LogLevel.None)
            return LogLevel.Fatal;
        if ((levels & LogLevel.Error) != LogLevel.None)
            return LogLevel.Error;
        if ((levels & LogLevel.Warning) != LogLevel.None)
            return LogLevel.Warning;
        if ((levels & LogLevel.Message) != LogLevel.None)
            return LogLevel.Message;
        if ((levels & LogLevel.Info) != LogLevel.None)
            return LogLevel.Info;
        if ((levels & LogLevel.Debug) != LogLevel.None)
            return LogLevel.Debug;
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
