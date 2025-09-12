using Hikaria.QC.Utilities;
using System;

namespace Hikaria.QC.Parsers
{
    public class EnumParser : PolymorphicCachedQcParser<Enum>
    {
        public override Enum Parse(string value, Type type)
        {
            try
            {
                return (Enum)Enum.Parse(type, value);
            }
            catch (Exception e)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(54, 
                    "Cannot parse '{0}' to the type '{1}'. To see the supported values, use the command enum-info '{2}'", value, type.GetDisplayName(), type), e);
            }
        }
    }
}