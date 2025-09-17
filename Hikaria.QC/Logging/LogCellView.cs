using Hikaria.ES;
using TMPro;
using UnityEngine;

namespace Hikaria.QC;

public class LogCellView : EnhancedScrollerCellView
{
    public TextMeshProUGUI LogText;

    private RectTransform _textRectTransform;
    private RectTransform _cellViewRectTransform;
    private RectTransform _viewportRectTransform;
    private LogCellData _logCellData;

    private void Awake()
    {
        _textRectTransform = transform.FindChild("Text").GetComponent<RectTransform>();
        LogText = _textRectTransform.GetComponent<TextMeshProUGUI>();
        _cellViewRectTransform = GetComponent<RectTransform>();
        _viewportRectTransform = QuantumConsole.Instance.ViewportRectTransform;
    }

    internal void Setup()
    {
        _textRectTransform = transform.FindChild("Text").GetComponent<RectTransform>();
        LogText = _textRectTransform.GetComponent<TextMeshProUGUI>();
        _cellViewRectTransform = GetComponent<RectTransform>();
    }

    public void SetData(LogCellData data, bool calculateLayout)
    {
        _logCellData = data;

        RefreshCellView();

        if (calculateLayout)
            UpdateLayout();
    }

    public override void RefreshCellView()
    {
        LogText.text = _logCellData.LogText;
    }

    public void UpdateLayout()
    {
        float parentWidth = _viewportRectTransform.rect.width;
        _cellViewRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);
        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);

        LogText.ForceMeshUpdate();

        Vector2 textSize = LogText.GetPreferredValues(parentWidth, float.MaxValue);
        float textHeight = textSize.y;

        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        _cellViewRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        _logCellData.CellSize = _textRectTransform.rect.height;
    }
}