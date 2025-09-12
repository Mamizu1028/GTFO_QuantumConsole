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
            Component objComponent = obj.GetComponent(Il2CppType.From(type, true));

            if (!objComponent)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(53, "No component on the object '{0}' of type {1} existed.", value, type.GetDisplayName()));
            }

            return objComponent;
        }
    }
}
