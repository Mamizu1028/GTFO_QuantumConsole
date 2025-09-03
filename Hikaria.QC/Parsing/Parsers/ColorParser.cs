using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hikaria.QC.Parsers
{
    public class ColorParser : BasicCachedQcParser<Color>
    {
        private readonly Dictionary<string, Color> _colorLookup;

        public ColorParser()
        {
            _colorLookup = new Dictionary<string, Color>();

            PropertyInfo[] colorProperties = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo prop in colorProperties)
            {
                if (prop.CanRead && !prop.CanWrite)
                {
                    MethodInfo propReader = prop.GetMethod;
                    if (propReader.ReturnType == typeof(Color))
                    {
                        _colorLookup.Add(prop.Name, (Color)propReader.Invoke(null, Array.Empty<object>()));
                    }
                }
            }
        }

        public override Color Parse(string value)
        {
            if (_colorLookup.ContainsKey(value.ToLower()))
            {
                return _colorLookup[value.ToLower()];
            }

            try
            {
                if (value.StartsWith("0x"))
                {
                    return ParseHexColor(value);
                }
                else
                {
                    return ParseRGBAColor(value);
                }
            }
            catch (FormatException e)
            {
                throw new ParserInputException(QuantumGlobal.Localization.Format(47, e.Message), e);
            }
        }

        private Color ParseRGBAColor(string value)
        {
            string[] colorParts = value.Split(',');
            Color parsedColor = Color.white;
            int i = 0;

            if (colorParts.Length < 3 || colorParts.Length > 4) { throw new FormatException(QuantumGlobal.Localization.Get(48)); }

            float ParsePart(string part)
            {
                float val = float.Parse(part);
                if (val < 0 || val > 1) { throw new FormatException(QuantumGlobal.Localization.Format(49, val)); }
                return val;
            }

            try
            {
                for (; i < colorParts.Length; i++)
                {
                    parsedColor[i] = ParsePart(colorParts[i]);
                }

                return parsedColor;
            }
            catch (FormatException)
            {
                throw new FormatException(QuantumGlobal.Localization.Format(50, colorParts[i]));
            }
        }

        private Color ParseHexColor(string value)
        {
            int digitCount = value.Length - 2;
            if (digitCount != 6 && digitCount != 8)
            {
                throw new FormatException(QuantumGlobal.Localization.Get(51));
            }

            Color parsedColor = Color.white;
            int byteCount = digitCount / 2;
            int i = 0;

            try
            {
                for (; i < byteCount; i++)
                {
                    parsedColor[i] = int.Parse(value.Substring(2 * (1 + i), 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                }

                return parsedColor;
            }
            catch (FormatException)
            {
                throw new FormatException(QuantumGlobal.Localization.Format(52, value.Substring(2 * (1 + i), 2)));
            }
        }
    }
}
