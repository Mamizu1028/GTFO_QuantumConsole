using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hikaria.QC
{
    internal sealed class LogTextLayoutCalculator
    {
        private const int _maxCacheEntries = 4096;

        private readonly TextMeshProUGUI _measurer;
        private readonly RectTransform _viewport;
        private readonly Dictionary<long, float> _cache;
        private readonly GameObject _measurerGO;

        private int _cachedWidthBucket = -1;

        public LogTextLayoutCalculator(LogCellView prefab, RectTransform viewport)
        {
            _viewport = viewport;
            _cache = new Dictionary<long, float>(256);

            Transform srcText = prefab.transform.FindChild("Text");
            _measurerGO = Object.Instantiate(srcText.gameObject);
            _measurerGO.name = "_QC_LogLayoutMeasurer";
            _measurerGO.SetActive(false);
            _measurerGO.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            Object.DontDestroyOnLoad(_measurerGO);

            _measurer = _measurerGO.GetComponent<TextMeshProUGUI>();
            _measurer.raycastTarget = false;
            _measurer.enabled = false;
        }

        public float CurrentWidth => _viewport.rect.width;

        public float Measure(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0f;

            float width = CurrentWidth;
            if (width <= 0f) return 0f;

            int widthBucket = Mathf.RoundToInt(width);
            if (_cachedWidthBucket != widthBucket)
            {
                _cache.Clear();
                _cachedWidthBucket = widthBucket;
            }

            long key = ComputeKey(text);
            if (_cache.TryGetValue(key, out float cached))
                return cached;

            _measurer.text = text;
            float height = _measurer.GetPreferredValues(width, float.MaxValue).y;

            if (_cache.Count < _maxCacheEntries)
                _cache[key] = height;

            return height;
        }

        public void InvalidateCache()
        {
            _cache.Clear();
            _cachedWidthBucket = -1;
        }

        public void Dispose()
        {
            if (_measurerGO != null)
                Object.Destroy(_measurerGO);
            _cache.Clear();
        }

        private static long ComputeKey(string text)
        {
            ulong hash = 14695981039346656037UL;
            int len = text.Length;
            for (int i = 0; i < len; i++)
            {
                hash ^= text[i];
                hash *= 1099511628211UL;
            }
            return unchecked((long)hash);
        }
    }
}
