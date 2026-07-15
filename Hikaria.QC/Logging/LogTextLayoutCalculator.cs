using TMPro;
using UnityEngine;

namespace Hikaria.QC
{
    internal sealed class LogTextLayoutCalculator
    {
        private readonly TextMeshProUGUI _measurer;
        private readonly RectTransform _viewport;
        private readonly GameObject _measurerGO;

        public LogTextLayoutCalculator(LogCellView prefab, RectTransform viewport)
        {
            _viewport = viewport;
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
        
        public bool TryUpdateViewportWidth()
        {
            float width = _viewport.rect.width;
            if (width <= 0f)
                return false;

            CurrentWidth = width;
            return true;
        }

        public float CurrentWidth { get; private set; }
        public bool IsReady => CurrentWidth > 0f;

        public float Measure(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0f;

            float width = CurrentWidth;
            if (width <= 0f) return 0f;

            _measurer.text = text;
            float height = _measurer.GetPreferredValues(width, float.MaxValue).y;

            return height;
        }
    }
}
