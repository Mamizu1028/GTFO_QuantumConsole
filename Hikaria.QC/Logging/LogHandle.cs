using System;

namespace Hikaria.QC;

public readonly struct LogHandle : IEquatable<LogHandle>
{
    public static readonly LogHandle Invalid = new LogHandle(0);

    public long Value { get; }
    public bool IsValid => Value > 0;

    internal LogHandle(long value)
    {
        Value = value;
    }

    public bool Equals(LogHandle other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is LogHandle other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(LogHandle left, LogHandle right) => left.Equals(right);
    public static bool operator !=(LogHandle left, LogHandle right) => !left.Equals(right);
}
