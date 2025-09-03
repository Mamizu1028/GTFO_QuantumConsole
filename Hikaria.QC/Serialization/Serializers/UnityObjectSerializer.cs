using UnityEngine;

namespace Hikaria.QC.Serializers
{
    public class UnityObjectSerializer : PolymorphicQcSerializer<Object>
    {
        public override string SerializeFormatted(Object value, QuantumTheme theme)
        {
            return value.name;
        }
    }
}
