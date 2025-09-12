using Hikaria.QC.Utilities;
using UnityEngine;

namespace Hikaria.QC.Parsers
{
    public class GameObjectParser : BasicQcParser<GameObject>
    {
        public override GameObject Parse(string value)
        {
            string name = ParseRecursive<string>(value);
            GameObject obj = GameObjectExtensions.Find(name, true);

            if (!obj)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(55, "Could not find GameObject of name {0}.", value));
            }

            return obj;
        }
    }
}
