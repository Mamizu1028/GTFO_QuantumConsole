using Hikaria.QC.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hikaria.QC
{
    public class QuantumTheme
    {
        public TMP_FontAsset Font => QuantumConsoleSettings.GetLoadedAsset(FontAssetPath).Cast<TMP_FontAsset>();
        public Material PanelMaterial => QuantumConsoleSettings.GetLoadedAsset(PanelMaterialAssetPath).Cast<Material>();
        public GameObject LogCellViewPrefab => QuantumConsoleSettings.GetLoadedAsset(LogCellViewPrefabAssetPath).Cast<GameObject>();

        public const string FontAssetPath = "Assets/Plugins/QFSW/Quantum Console/Source/Fonts/TMP/OfficeCodePro-Regular SDF.asset";
        public const string PanelMaterialAssetPath = "Assets/Plugins/QFSW/Quantum Console/Source/Materials/Blur Panel.mat";
        public const string LogCellViewPrefabAssetPath = "Assets/Plugins/QFSW/Quantum Console/Source/Prefabs/LogCellView.prefab";

        public Color PanelColor = ColorExtensions.WHITE;

        public Color CommandLogColor = ColorExtensions.BRIGHT_CYAN;
        public Color SelectedSuggestionColor = ColorExtensions.DARK_YELLOW;
        public Color SuggestionColor = ColorExtensions.DARK_GRAY;
        public Color ErrorColor = ColorExtensions.DARK_RED;
        public Color FatalErrorColor = ColorExtensions.BRIGHT_RED;
        public Color WarningColor = ColorExtensions.BRIGHT_YELLOW;
        public Color MessageColor = ColorExtensions.WHITE;
        public Color DebugColor = ColorExtensions.DARK_GRAY;
        public Color InfoColor = ColorExtensions.BRIGHT_GRAY;
        public Color SuccessColor = ColorExtensions.BRIGHT_GREEN;

        public string TimestampFormat = "[{0:00}:{1:00}:{2:00}]";
        public string CommandLogFormat = "> {0}";

        public Color DefaultReturnValueColor = ColorExt.Hex("FFBC8C");
        public List<TypeColorFormatter> TypeFormatters = new List<TypeColorFormatter>(0);
        public List<CollectionFormatter> CollectionFormatters = new List<CollectionFormatter>(0);

        private T FindTypeFormatter<T>(List<T> formatters, Type type) where T : TypeFormatter
        {
            foreach (T formatter in formatters)
            {
                if (type == formatter.Type || type.IsGenericTypeOf(formatter.Type))
                {
                    return formatter;
                }
            }

            foreach (T formatter in formatters)
            {
                if (formatter.Type.IsAssignableFrom(type))
                {
                    return formatter;
                }
            }

            return null;
        }

        public string ColorizeReturn(string data, Type type)
        {
            TypeColorFormatter formatter = FindTypeFormatter(TypeFormatters, type);
            if (formatter == null) { return data.ColorText(DefaultReturnValueColor); }
            else { return data.ColorText(formatter.Color); }
        }

        public void GetCollectionFormatting(Type type, out string leftScoper, out string seperator, out string rightScoper)
        {
            CollectionFormatter formatter = FindTypeFormatter(CollectionFormatters, type);
            if (formatter == null)
            {
                leftScoper = "[";
                seperator = ",";
                rightScoper = "]";
            }
            else
            {
                leftScoper = formatter.LeftScoper.Replace("\\n", "\n");
                seperator = formatter.SeperatorString.Replace("\\n", "\n");
                rightScoper = formatter.RightScoper.Replace("\\n", "\n");
            }
        }

        public static QuantumTheme DefaultTheme()
        {
            var theme = new QuantumTheme();
            theme.TypeFormatters.Add(new TypeColorFormatter(typeof(string))
            {
                Color = ColorExtensions.WHITE
            });
            theme.TypeFormatters.Add(new TypeColorFormatter(typeof(IEnumerable))
            {
                Color = ColorExt.Hex("FDF269")
            });
            theme.TypeFormatters.Add(new TypeColorFormatter(typeof(KeyValuePair))
            {
                Color = ColorExt.Hex("BAFFEE")
            });
            theme.TypeFormatters.Add(new TypeColorFormatter(typeof(DictionaryEntry))
            {
                Color = ColorExt.Hex("BAFFEE")
            });
            theme.TypeFormatters.Add(new TypeColorFormatter(typeof(Enum))
            {
                Color = ColorExt.Hex("C4FF8C")
            });
            theme.TypeFormatters.Add(new TypeColorFormatter(typeof(UnityEngine.Object))
            {
                Color = ColorExt.Hex("F799FF")
            });
            theme.CollectionFormatters.Add(new CollectionFormatter(typeof(Dictionary<,>))
            {
                LeftScoper = string.Empty,
                RightScoper = string.Empty,
                SeperatorString = "\n"
            });
            theme.CollectionFormatters.Add(new CollectionFormatter(typeof(ICollection))
            {
                LeftScoper = "[",
                RightScoper = "]",
                SeperatorString = ","
            });
            theme.CollectionFormatters.Add(new CollectionFormatter(typeof(IEnumerable))
            {
                LeftScoper = string.Empty,
                RightScoper = string.Empty,
                SeperatorString = "\n"
            });
            return theme;
        }
    }
}
