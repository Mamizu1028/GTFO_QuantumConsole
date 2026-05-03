using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hikaria.QC
{
    /// <summary>
    /// 在主线程上、利用一个隐藏的 <see cref="TextMeshProUGUI"/> 镜像 LogCellView 的 TMP 配置，
    /// 在日志写入时即提前算好它在当前视口宽度下的高度。
    /// 把测量从渲染热路径（EnhancedScroller 的 GetCellViewSize / cell SetData）移到了数据写入路径。
    /// </summary>
    /// <remarks>
    /// 仅可在 Unity 主线程上调用。
    /// </remarks>
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

            // 通过克隆 prefab 上的 Text 子节点取得"完全一致的 TMP 配置"——
            // fontAsset / fontSize / fontStyle / lineSpacing / characterSpacing /
            // paragraphSpacing / margin / alignment / richText / spriteAsset 等
            // 全部在 GameObject 层面整体 clone，无需手动逐字段 copy。
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

        /// <summary>当前视口宽度。≤ 0 时表示尚未初始化。</summary>
        public float CurrentWidth => _viewport.rect.width;

        /// <summary>测量给定文本在当前视口宽度下的高度（带 (text, widthBucket) 命中缓存）。</summary>
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

        /// <summary>视口宽度变化或字体属性变更时调用。</summary>
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

        // FNV-1a 64-bit。对于日志文本，碰撞概率与 SHA 不在同一量级，但
        // 在 4096 条缓存上限下完全够用，且零分配、O(n)。
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
