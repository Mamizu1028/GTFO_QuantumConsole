using System;
using TheArchive.Interfaces;

namespace Hikaria.QC;

public static class Logs
{
    private static IArchiveLogger _logger;

    public static void Setup(IArchiveLogger logger)
    {
        _logger = logger;
    }

    public static void Debug(object data)
    {
        _logger.Debug(data.ToString());
    }

    public static void Error(object data)
    {
        _logger.Error(data.ToString());
    }

    public static void Info(object data)
    {
        _logger.Info(data.ToString());
    }

    public static void Message(object data)
    {
        _logger.Msg(ConsoleColor.White, data.ToString());
    }

    public static void Warning(object data)
    {
        _logger.Warning(data.ToString());
    }

    public static void Notice(object data)
    {
        _logger.Notice(data.ToString());
    }

    public static void Success(object data)
    {
        _logger.Success(data.ToString());
    }

    public static void Exception(Exception ex)
    {
        _logger.Exception(ex);
    }
}
