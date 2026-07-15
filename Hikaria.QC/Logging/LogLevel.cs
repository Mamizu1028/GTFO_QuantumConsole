using System;

namespace Hikaria.QC;

[Flags]
public enum LogLevel : byte
{
    None = 0,
    Fatal = 1 << 0,
    Error = 1 << 1,
    Warning = 1 << 2,
    Message = 1 << 3,
    Info = 1 << 4,
    Debug = 1 << 5,
    All = Fatal | Error | Warning | Message | Info | Debug
}