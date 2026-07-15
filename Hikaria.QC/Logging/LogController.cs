using Hikaria.ES;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hikaria.QC
{
    internal class LogController : ILogController, IEnhancedScrollerDelegate
    {
        private readonly LogStorage _logStorage;
        private readonly List<LogOperation> _pendingOperations = new List<LogOperation>(1024);
        private readonly List<Log> _dirtyLogs = new List<Log>(256);
        private readonly HashSet<LogHandle> _dirtyLogHandles = new HashSet<LogHandle>();

        private readonly EnhancedScroller _scroller;
        private readonly LogCellView _logCellViewPrefab;
        private readonly LogTextLayoutCalculator _layoutCalc;
        private readonly LogViewingLatestTracker _viewingLatestTracker = new LogViewingLatestTracker();

        private bool _isDirty;
        private bool _viewportSizeChanged;
        private bool _needScrollToLatest;
        private bool _immediateScroll;
        private bool _reloadDataNeeded;
        private bool _layoutRefreshPending;

        private const float _immediateTweenTime = 0.1f;
        private readonly float _tweenTime;
        private readonly EnhancedScroller.TweenType _tweenType;

        public int MaxHistoryLogs
        {
            get => _logStorage.MaxHistoryLogs;
            set => _logStorage.MaxHistoryLogs = value;
        }

        public bool IsDirty => _isDirty;

        public LogController(EnhancedScroller scroller, LogCellView logCellViewPrefab,
            RectTransform viewport, int maxStoredLogs = LogStorage.DefaultMaxHistoryLogs,
            EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.easeOutSine,
            float tweenTime = 0.5f)
        {
            _logStorage = new LogStorage(maxStoredLogs);

            _scroller = scroller;
            _scroller.Delegate = this;
            _logCellViewPrefab = logCellViewPrefab;
            _layoutCalc = new LogTextLayoutCalculator(logCellViewPrefab, viewport);

            _tweenTime = tweenTime;
            _tweenType = tweenType;

            _scroller.interruptTweeningOnDrag = true;
            _scroller.interruptTweeningOnPointerDown = true;
        }

        public void ProcessLogs()
        {
            int storageCountBefore = _logStorage.Count;
            bool scrollerDataStale = _viewingLatestTracker.ScrollerDataStale;
            _viewingLatestTracker.RefreshFromScrollerUnlessStale(_scroller.IsTweening, storageCountBefore, _scroller.NumberOfCells > 0 ? _scroller.EndDataIndex : 0);
            float scrollPosition = _scroller.ScrollPosition;
            float linearVelocity = _scroller.LinearVelocity;
            _needScrollToLatest |= _viewingLatestTracker.IsViewingLatestLog;
            bool hadPendingOperations = _pendingOperations.Count > 0;
            _reloadDataNeeded = false;
            _logStorage.ClearTrimmedHistoryLogs();

            for (int i = 0; i < _pendingOperations.Count; i++)
                ApplyOperation(_pendingOperations[i]);
            _pendingOperations.Clear();
            float trimmedHistoryExtent = _logStorage.TrimmedHistoryExtent;

            if (_layoutRefreshPending)
            {
                MarkAllDirty();
                _reloadDataNeeded = true;
                _layoutRefreshPending = false;
            }

            if (_viewportSizeChanged)
            {
                if (!_layoutCalc.TryUpdateViewportWidth())
                {
                    DeferLayoutRefresh();
                    return;
                }

                MarkAllDirty();
                _reloadDataNeeded = true;
            }

            if (!_layoutCalc.IsReady && _logStorage.Count > 0)
            {
                DeferLayoutRefresh();
                return;
            }

            bool cellSizeChanged = RemeasureDirty();
            bool storageCountChangedThisPass = storageCountBefore != _logStorage.Count;
            bool reloadData = scrollerDataStale || _reloadDataNeeded || cellSizeChanged || storageCountChangedThisPass;

            if (reloadData)
            {
                _scroller.ScrollPosition = 0;
                _scroller.ReloadData();

                if (_logStorage.Count > 0)
                {
                    float restoredPosition = Mathf.Max(0f, scrollPosition - trimmedHistoryExtent);
                    _scroller.SetScrollPositionImmediately(restoredPosition);
                    _scroller.LinearVelocity = linearVelocity;

                    if (_needScrollToLatest)
                    {
                        _scroller.JumpToDataIndex(_logStorage.Count - 1, 1f, 1f, false, _tweenType,
                            _immediateScroll ? _immediateTweenTime : _tweenTime,
                            forceCalculateRange: true);
                    }
                }
            }
            else if (hadPendingOperations)
            {
                _scroller.RefreshActiveCellViews();
            }

            _isDirty = false;
            _viewportSizeChanged = false;
            _needScrollToLatest = false;
            _immediateScroll = false;
            _viewingLatestTracker.MarkScrollerDataCurrent();

            _logStorage.ClearTrimmedHistoryLogs();
        }

        public void ProcessStorage()
        {
            if (_pendingOperations.Count == 0)
                return;

            _viewingLatestTracker.CaptureBeforeStorageOnlyProcessing(_scroller.IsTweening, _logStorage.Count, _scroller.NumberOfCells > 0 ? _scroller.EndDataIndex : 0);

            _reloadDataNeeded = false;
            _logStorage.ClearTrimmedHistoryLogs();

            for (int i = 0; i < _pendingOperations.Count; i++)
                ApplyOperation(_pendingOperations[i]);

            _pendingOperations.Clear();

            bool canMeasureDirtyLogs = _layoutCalc.IsReady && !_layoutRefreshPending;
            if (canMeasureDirtyLogs)
            {
                RemeasureDirty();
                _layoutRefreshPending = false;
            }
            else
            {
                _dirtyLogs.Clear();
                _dirtyLogHandles.Clear();
                _layoutRefreshPending = true;
            }

            _logStorage.ClearTrimmedHistoryLogs();

            _reloadDataNeeded = false;
            _isDirty = true;
        }

        public void EnqueueOperation(LogOperation operation)
        {
            _pendingOperations.Add(operation);
            _isDirty = true;
        }

        public void Clear(LogClearScope scope)
        {
            EnqueueOperation(LogOperation.Clear(scope));
        }

        public void RebuildLogTextLayout(bool viewportSizeChanged)
        {
            _isDirty = true;
            _viewportSizeChanged |= viewportSizeChanged;
        }

        public void ScrollToLatest(bool immediate)
        {
            _needScrollToLatest = true;
            _immediateScroll = immediate;
            _isDirty = true;
        }

        public int GetNumberOfCells(EnhancedScroller scroller) => _logStorage.Count;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
            => GetEntry(dataIndex).CellSize;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            LogCellView cellView = (LogCellView)scroller.GetCellView(_logCellViewPrefab);
            cellView.SetData(GetEntry(dataIndex), _layoutCalc.CurrentWidth);
            return cellView;
        }

        private void ApplyOperation(LogOperation operation)
        {
            switch (operation.Kind)
            {
                case LogOperationKind.AddHistory:
                    if (_logStorage.AddHistory(operation.Id, operation.Text, operation.Level))
                    {
                        MarkLogDirty(operation.Id);
                        _reloadDataNeeded = true;
                    }
                    break;
                case LogOperationKind.BeginHistoryStream:
                    if (_logStorage.BeginHistoryStream(operation.Id, operation.Text, operation.Level))
                    {
                        MarkLogDirty(operation.Id);
                        _reloadDataNeeded = true;
                    }
                    break;
                case LogOperationKind.AddLiveStatus:
                    if (_logStorage.AddLive(operation.Id, operation.Text, operation.Level))
                    {
                        MarkLogDirty(operation.Id);
                        _reloadDataNeeded = true;
                    }
                    break;
                case LogOperationKind.AddInteractive:
                    if (_logStorage.AddInteractive(operation.Id, operation.Text, operation.Level))
                    {
                        MarkLogDirty(operation.Id);
                        _reloadDataNeeded = true;
                    }
                    break;
                case LogOperationKind.Update:
                    if (_logStorage.Update(operation.Id, operation.Text, operation.Level))
                        MarkLogDirty(operation.Id);
                    break;
                case LogOperationKind.Append:
                    if (_logStorage.Append(operation.Id, operation.Text))
                        MarkLogDirty(operation.Id);
                    break;
                case LogOperationKind.Complete:
                    if (_logStorage.Complete(operation.Id, out Log? affectedLog))
                    {
                        if (affectedLog != null)
                            MarkDirty(affectedLog);

                        _reloadDataNeeded = true;
                    }
                    break;
                case LogOperationKind.Commit:
                    if (_logStorage.Commit(operation.Id,
                        operation.HasFinalText ? operation.Text : null,
                        operation.Level, out Log? committedLog))
                    {
                        if (committedLog != null)
                            MarkDirty(committedLog);

                        _reloadDataNeeded = true;
                    }
                    break;
                case LogOperationKind.Remove:
                    if (_logStorage.Remove(operation.Id))
                        _reloadDataNeeded = true;
                    break;
                case LogOperationKind.Clear:
                    _logStorage.Clear(operation.ClearScope);
                    _dirtyLogs.Clear();
                    _dirtyLogHandles.Clear();
                    _reloadDataNeeded = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Log GetEntry(int dataIndex)
        {
            return _logStorage[dataIndex];
        }

        private void DeferLayoutRefresh()
        {
            _layoutRefreshPending = true;
            _isDirty = true;
        }

        private bool RemeasureDirty()
        {
            bool cellSizeChanged = false;
            for (int i = 0; i < _dirtyLogs.Count; i++)
            {
                Log log = _dirtyLogs[i];
                if (!log.IsDirty)
                    continue;

                float cellSize = _layoutCalc.Measure(log.Text);
                if (Mathf.Abs(log.CellSize - cellSize) > 0.01f)
                {
                    log.CellSize = cellSize;
                    cellSizeChanged = true;
                }

                log.IsDirty = false;
            }

            _dirtyLogs.Clear();
            _dirtyLogHandles.Clear();
            return cellSizeChanged;
        }

        private void MarkAllDirty()
        {
            for (int i = 0; i < _logStorage.Count; i++)
                MarkDirty(_logStorage[i]);
        }

        private void MarkLogDirty(LogHandle id)
        {
            if (_logStorage.TryGet(id, out Log log))
                MarkDirty(log);
        }

        private void MarkDirty(Log log)
        {
            log.IsDirty = true;
            if (_dirtyLogHandles.Add(log.Id))
                _dirtyLogs.Add(log);
        }
    }
}
