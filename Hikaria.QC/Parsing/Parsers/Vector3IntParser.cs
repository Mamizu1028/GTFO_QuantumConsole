using UnityEngine;

namespace Hikaria.QC.Parsers
{
    public class Vector3IntParser : BasicCachedQcParser<Vector3Int>
    {
        public override Vector3Int Parse(string value)
        {
            string[] vectorParts = value.Split(',');
            Vector3Int parsedVector = new Vector3Int();

            if (vectorParts.Length < 2 || vectorParts.Length > 3)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(58, value));
            }

            int i = 0;
            try
            {
                for (; i < vectorParts.Length; i++)
                {
                    parsedVector[i] = int.Parse(vectorParts[i]);
                }

                return parsedVector;
            }
            catch
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(59, vectorParts[i]));
            }
        }
    }
}