using Hikaria.QC.Utilities;
using Il2CppInterop.Runtime;
using System;
using UnityEngine;

namespace Hikaria.QC.Parsers
{
    public class ComponentParser : PolymorphicQcParser<Component>
    {
        public override Component Parse(string value, Type type)
        {
            GameObject obj = ParseRecursive<GameObject>(value);
            Component objComponent = obj.GetComponent(Il2CppType.From(type));

            if (!objComponent)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(53, value, type.GetDisplayName()));
            }

            return objComponent;
        }
    }
}
