using Hikaria.QC.Pooling;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine;

namespace Hikaria.QC.Utilities
{
    public static class ColorExtensions
    {
        private static readonly ConcurrentStringBuilderPool _stringBuilderPool = new ConcurrentStringBuilderPool();

        /// <summary>Colors a string using rich formatting.</summary>
        /// <returns>The formatted text.</returns>
        /// <param name="text">The text to color.</param>
        /// <param name="color">The color to add to the text.</param>
        public static string ColorText(this string text, Color color)
        {
            StringBuilder buffer = _stringBuilderPool.GetStringBuilder(text.Length + 10);
            buffer.AppendColoredText(text, color);
            return _stringBuilderPool.ReleaseAndToString(buffer);
        }

        /// <summary>Colors a string using rich formatting and inserts the result into a string builder.</summary>
        /// <returns>The formatted text.</returns>
        /// <param name="stringBuilder">String builder to add the result into</param>
        /// <param name="text">The text to color.</param>
        /// <param name="color">The color to add to the text.</param>
        public static void AppendColoredText(this StringBuilder stringBuilder, string text, Color color)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                stringBuilder.Append(text);
                return;
            }

            string hexColor = Color32ToStringNonAlloc(color);
            stringBuilder.Append("<color=#");
            stringBuilder.Append(hexColor);
            stringBuilder.Append('>');
            stringBuilder.Append(text);
            stringBuilder.Append("</color>");
        }

        private static readonly ConcurrentDictionary<int, string> _colorLookupTable = new ConcurrentDictionary<int, string>();
        public static unsafe string Color32ToStringNonAlloc(Color32 color)
        {
            int colorKey = color.r << 24 | color.g << 16 | color.b << 8 | color.a;
            if (_colorLookupTable.TryGetValue(colorKey, out string cachedColor))
            {
                return cachedColor;
            }

            char* buffer = stackalloc char[8];
            Color32ToHexNonAlloc(color, buffer);

            int bufferLength = color.a < 0xFF ? 8 : 6;
            string colorText = new string(buffer, 0, bufferLength);

            _colorLookupTable.TryAdd(colorKey, colorText);
            return colorText;
        }

        private static unsafe void Color32ToHexNonAlloc(Color32 color, char* buffer)
        {
            ByteToHex(color.r, out buffer[0], out buffer[1]);
            ByteToHex(color.g, out buffer[2], out buffer[3]);
            ByteToHex(color.b, out buffer[4], out buffer[5]);
            ByteToHex(color.a, out buffer[6], out buffer[7]);
        }

        private static void ByteToHex(byte value, out char dig1, out char dig2)
        {
            dig1 = NibbleToHex((byte)(value >> 4));
            dig2 = NibbleToHex((byte)(value & 0xF));
        }

        private static char NibbleToHex(byte nibble)
        {
            if (nibble < 10) { return (char)('0' + nibble); }
            else { return (char)('A' + nibble - 10); }
        }

        public static readonly Color BLACK = new Color(12f / 255f, 12f / 255f, 12f / 255f);
        public static readonly Color DARK_BLUE = new Color(0f / 255f, 55f / 255f, 218f / 255f);
        public static readonly Color DARK_GREEN = new Color(19f / 255f, 161f / 255f, 14f / 255f);
        public static readonly Color DARK_CYAN = new Color(58f / 255f, 150f / 255f, 221f / 255f);
        public static readonly Color DARK_RED = new Color(197f / 255f, 15f / 255f, 31f / 255f);
        public static readonly Color DARK_MAGENTA = new Color(136f / 255f, 23f / 255f, 152f / 255f);
        public static readonly Color DARK_YELLOW = new Color(193f / 255f, 156f / 255f, 0f / 255f);
        public static readonly Color DARK_WHITE = new Color(204f / 255f, 204f / 255f, 204f / 255f);
        public static readonly Color BRIGHT_BLACK = new Color(118f / 255f, 118f / 255f, 118f / 255f);
        public static readonly Color BRIGHT_BLUE = new Color(59f / 255f, 120f / 255f, 255f / 255f);
        public static readonly Color BRIGHT_GREEN = new Color(22f / 255f, 198f / 255f, 12f / 255f);
        public static readonly Color BRIGHT_CYAN = new Color(97f / 255f, 214f / 255f, 214f / 255f);
        public static readonly Color BRIGHT_RED = new Color(231f / 255f, 72f / 255f, 86f / 255f);
        public static readonly Color BRIGHT_MAGENTA = new Color(180f / 255f, 0f / 255f, 158f / 255f);
        public static readonly Color BRIGHT_YELLOW = new Color(249f / 255f, 241f / 255f, 165f / 255f);
        public static readonly Color WHITE = new Color(242f / 255f, 242f / 255f, 242f / 255f);

        public static Color DARK_GRAY => BRIGHT_BLACK;
        public static Color BRIGHT_GRAY => DARK_WHITE;
    }
}
