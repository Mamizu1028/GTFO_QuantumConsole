using System;
using System.Collections.Generic;

namespace Hikaria.QC.Utilities
{
    public static class EnumFlagExtensions
    {
        public static TFlags ToFlags<TFlags>(this List<TFlags> enums) where TFlags : Enum
        {
            if (enums == null) throw new ArgumentNullException(nameof(enums));

            ulong result = 0;

            foreach (var e in enums)
            {
                if (e is TFlags flag)
                {
                    result |= Convert.ToUInt64(flag);
                }
                else
                {
                    throw new ArgumentException($"All items in the list must be of type {typeof(TFlags).Name}", nameof(enums));
                }
            }

            return (TFlags)Enum.ToObject(typeof(TFlags), result);
        }
    }
}
