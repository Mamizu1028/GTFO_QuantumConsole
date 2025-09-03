using UnityEngine;

namespace Hikaria.QC.Parsers
{
    public class Vector4Parser : BasicCachedQcParser<Vector4>
    {
        public override Vector4 Parse(string value)
        {
            string[] vectorParts = value.SplitScoped(',');
            Vector4 parsedVector = new Vector4();

            if (vectorParts.Length < 2 || vectorParts.Length > 4)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(60, value));
            }

            for (int i = 0; i < vectorParts.Length; i++)
            {
                parsedVector[i] = ParseRecursive<float>(vectorParts[i]);
            }

            return parsedVector;
        }
    }
}