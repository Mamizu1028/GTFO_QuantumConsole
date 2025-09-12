using Hikaria.QC.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hikaria.QC.Parsers
{
    public class PrimitiveParser : IQcParser
    {
        private readonly HashSet<Type> _primitiveTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(float),
            typeof(decimal),
            typeof(double),
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(uint),
            typeof(short),
            typeof(ushort),
            typeof(long),
            typeof(ulong),
            typeof(char)
        };

        public int Priority => -1000;

        public bool CanParse(Type type)
        {
            return _primitiveTypes.Contains(type);
        }

        public object Parse(string value, Type type, Func<string, Type, object> recursiveParser)
        {
            try
            {
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(56, "Cannot parse '{0}' to the type '{1}'.", value, type.GetDisplayName()), e);
            }
        }
    }
}
