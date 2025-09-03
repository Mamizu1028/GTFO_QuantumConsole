using System;

namespace Hikaria.QC.Grammar
{
    public interface IBinaryOperator
    {
        Type LArg { get; }
        Type RArg { get; }
        Type Ret { get; }

        object Invoke(object left, object right);
    }
}
