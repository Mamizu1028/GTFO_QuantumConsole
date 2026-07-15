namespace Hikaria.QC;

internal sealed class LogViewingLatestTracker
{
    private bool _scrollerDataStale;

    public bool IsViewingLatestLog { get; private set; }
    public bool ScrollerDataStale => _scrollerDataStale;

    public void CaptureBeforeStorageOnlyProcessing(bool isTweening, int storageCount, int scrollerEndDataIndex)
    {
        if (!_scrollerDataStale)
            CaptureFromCurrentScroller(isTweening, storageCount, scrollerEndDataIndex);

        _scrollerDataStale = true;
    }

    public void RefreshFromScrollerUnlessStale(bool isTweening, int storageCount, int scrollerEndDataIndex)
    {
        if (!_scrollerDataStale)
            CaptureFromCurrentScroller(isTweening, storageCount, scrollerEndDataIndex);
    }

    public void MarkScrollerDataCurrent()
    {
        _scrollerDataStale = false;
    }

    private void CaptureFromCurrentScroller(bool isTweening, int storageCount, int scrollerEndDataIndex)
    {
        if (isTweening)
            return;

        IsViewingLatestLog = storageCount == 0 || scrollerEndDataIndex == storageCount - 1;
    }
}
