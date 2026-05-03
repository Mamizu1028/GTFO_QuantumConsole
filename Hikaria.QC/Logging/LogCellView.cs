using Hikaria.ES;
using TMPro;
using UnityEngine;

namespace Hikaria.QC;

public class LogCellView : EnhancedScrollerCellView
{
    public TextMeshProUGUI LogText;

    private RectTransform _textRectTransform;
    private RectTransform _cellViewRectTransform;
    private LogCellData _logCellData;
    private float _appliedWidth = -1f;
    private float _appliedHeight = -1f;

    private void Awake()
    {
        EnsureCachedRefs();
    }

    internal void Setup()
    {
        EnsureCachedRefs();
    }

    private void EnsureCachedRefs()
    {
        if (_textRectTransform != null) return;

        _textRectTransform = transform.FindChild("Text").GetComponent<RectTransform>();
        LogText = _textRectTransform.GetComponent<TextMeshProUGUI>();
        _cellViewRectTransform = GetComponent<RectTransform>();
    }

    public void SetData(LogCellData data, float width)
    {
        _logCellData = data;

        string s = data.GetLogString();
        if (LogText.text != s)
            LogText.text = s;

        if (_appliedWidth != width)
        {
            _cellViewRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _appliedWidth = width;
        }

        float height = data.CellSize;
        if (_appliedHeight != height)
        {
            _cellViewRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            _appliedHeight = height;
        }
    }

    public override void RefreshCellView()
    {
        if (_logCellData == null) return;

        string s = _logCellData.GetLogString();
        if (LogText.text != s)
            LogText.text = s;
    }
}
