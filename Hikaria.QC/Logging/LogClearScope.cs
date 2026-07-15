using System;

namespace Hikaria.QC;

[Flags]
public enum LogClearScope
{
    None = 0,
    History = 1 << 0,
    Live = 1 << 1,
    All = History | Live
}
