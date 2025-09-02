using System.Collections;

namespace Hikaria.QC.Serializers
{
    public class DictionaryEntrySerializer : BasicQcSerializer<DictionaryEntry>
    {
        public override string SerializeFormatted(DictionaryEntry value, QuantumTheme theme)
        {
            string innerKey = SerializeRecursive(value.Key, theme);
            string innerValue = SerializeRecursive(value.Value, theme);

            return $"{innerKey}: {innerValue}";
        }
    }
}
