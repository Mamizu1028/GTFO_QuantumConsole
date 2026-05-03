using Hikaria.ES;
using Hikaria.QC.Pooling;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hikaria.QC
{
    internal class LogController : ILogController, IEnhancedScrollerDelegate
    {
        private readonly LogDataDeque _logDatas = new LogDataDeque(1024);

        private readonly List<ILog?> _pendingActions = new List<ILog?>(1024);

        private readonly EnhancedScroller _scroller;
        private readonly LogCellView _logCellViewPrefab;
        private readonly RectTransform _viewport;
        private readonly LogTextLayoutCalculator _layoutCalc;
        private readonly Pool<LogCellData> _cellDataPool = new Pool<LogCellData>();

        private int _logDataIndexOffset;
        private float _scrollPositionOffset;
        private int _logCountDelta;
        private int _logDataCountDelta;

        private bool _isDirty;
        private bool _viewportSizeChanged;
        private bool _needScrollToLatest;
        private bool _immediateScroll;
        private bool _firstFlushPending = true;

        private const float _immediateTweenTime = 0.1f;
        private readonly float _tweenTime;
        private readonly EnhancedScroller.TweenType _tweenType;

        public bool IsViewingLatestLog { get; private set; }

        public int MaxStoredLogs
        {
            get => _logDatas.Capacity;
            set => _logDatas.Capacity = value;
        }

        public IReadOnlyList<ILogData> LogDatas => _logDatas;

        public bool IsDirty => _isDirty;

        public LogController(EnhancedScroller scroller, LogCellView logCellViewPrefab,
            RectTransform viewport, int maxStoredLogs = -1,
            EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.easeOutSine,
            float tweenTime = 0.5f)
        {
            _scroller = scroller;
            _scroller.Delegate = this;
            _logCellViewPrefab = logCellViewPrefab;
            _viewport = viewport;
            _layoutCalc = new LogTextLayoutCalculator(logCellViewPrefab, viewport);

            _logDatas.Capacity = maxStoredLogs;

            _tweenTime = tweenTime;
            _tweenType = tweenType;

            _scroller.interruptTweeningOnDrag = true;
            _scroller.interruptTweeningOnPointerDown = true;
        }

        public void ProcessLogs()
        {
            if (!_scroller.IsTweening)
                IsViewingLatestLog = _logDatas.Count == 0 || _scroller.EndDataIndex == _logDatas.Count - 1;

            int startDataIndex = _logDatas.Count == 0 ? 0 : _scroller.StartDataIndex + 1;
            float scrollPosition = _scroller.ScrollPosition;
            float linearVelocity = _scroller.LinearVelocity;
            _needScrollToLatest |= IsViewingLatestLog;

            int actionCount = _pendingActions.Count;
            for (int i = 0; i < actionCount; i++)
            {
                ILog? log = _pendingActions[i];
                if (log != null) AppendLogInternal(log);
                else             RemoveLastLogInternal();
            }
            _pendingActions.Clear();

            ApplyMaxStoredLogsTrim();

            if (_viewportSizeChanged)
            {
                _layoutCalc.InvalidateCache();
                RemeasureAll();
            }

            _scroller.ScrollPosition = 0;
            _scroller.ReloadData();

            if (_logDatas.Count > 0)
            {
                if (_viewportSizeChanged)
                {
                    startDataIndex = Math.Max(0, startDataIndex + _logDataIndexOffset);
                    _scroller.JumpToDataIndex(startDataIndex, 0f, 0f, false);
                }
                else
                {
                    _scroller.ScrollPosition = scrollPosition + _scrollPositionOffset;
                    _scroller.LinearVelocity = linearVelocity;
                }

                if (_needScrollToLatest)
                {
                    _scroller.JumpToDataIndex(_logDatas.Count - 1, 1f, 1f, false, _tweenType,
                        _immediateScroll ? _immediateTweenTime : _tweenTime,
                        forceCalculateRange: false, jumpComplete: JumpComplete);
                }
            }

            _isDirty = false;
            _viewportSizeChanged = false;
            _needScrollToLatest = false;
            _immediateScroll = false;
            _logDataIndexOffset = 0;
            _logCountDelta = 0;
            _logDataCountDelta = 0;
            _scrollPositionOffset = 0;

            if (_firstFlushPending && _logDatas.Count > 0)
            {
                _firstFlushPending = false;
                RemeasureAll();
                _isDirty = true;
            }

            void JumpComplete()
            {
                if (_logCountDelta == 0)
                    _scroller.RefreshActive();
            }
        }

        public void AddLog(ILog log)
        {
            _isDirty = true;
            _pendingActions.Add(log);
        }

        public void RemoveLog()
        {
            if (_logDatas.Count == 0 && !HasPendingAdd())
                return;

            _isDirty = true;
            _pendingActions.Add(null);
        }

        public void Clear()
        {
            _isDirty = false;
            _pendingActions.Clear();
            _logDataIndexOffset = 0;
            _logDataCountDelta = 0;
            _logCountDelta = 0;
            _scrollPositionOffset = 0;
            _scroller.ClearAll();

            int n = _logDatas.Count;
            for (int i = 0; i < n; i++)
                _cellDataPool.Release(_logDatas[i]);
            _logDatas.Clear();

            _scroller.ReloadData();
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

        public int GetNumberOfCells(EnhancedScroller scroller) => _logDatas.Count;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
            => _logDatas[dataIndex].CellSize;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            LogCellView cellView = (LogCellView)scroller.GetCellView(_logCellViewPrefab);
            cellView.SetData(_logDatas[dataIndex], _layoutCalc.CurrentWidth);
            return cellView;
        }

        private void AppendLogInternal(ILog log)
        {
            _logCountDelta++;

            LogCellData target;
            if (log.NewLine || _logDatas.Count == 0)
            {
                target = _cellDataPool.GetObject();
                target.Reset(log);
                _logDatas.AddLast(target);
                _logDataCountDelta++;
            }
            else
            {
                target = _logDatas.Last;
                target.AppendLog(log);
            }

            target.CellSize = _layoutCalc.Measure(target.GetLogString());
            target.IsDirty = false;
        }

        private void RemoveLastLogInternal()
        {
            if (_logDatas.Count == 0) return;

            var logData = _logDatas.Last;
            if (logData.RemoveLog())
                _logCountDelta--;

            if (logData.Logs.Count == 0)
            {
                _logDatas.RemoveLast();
                _logDataCountDelta--;
                _cellDataPool.Release(logData);
            }
            else
            {
                logData.CellSize = _layoutCalc.Measure(logData.GetLogString());
                logData.IsDirty = false;
            }
        }

        private void ApplyMaxStoredLogsTrim()
        {
            int max = _logDatas.Capacity;
            if (max <= 0) return;

            while (_logDatas.Count > max)
            {
                var dropped = _logDatas.RemoveFirst();
                _logCountDelta -= dropped.Logs.Count;
                _logDataCountDelta--;
                _logDataIndexOffset--;
                _scrollPositionOffset -= dropped.CellSize;
                _cellDataPool.Release(dropped);
            }
        }

        private void RemeasureAll()
        {
            int n = _logDatas.Count;
            for (int i = 0; i < n; i++)
            {
                var d = _logDatas[i];
                d.CellSize = _layoutCalc.Measure(d.GetLogString());
                d.IsDirty = false;
            }
        }

        private bool HasPendingAdd()
        {
            int n = _pendingActions.Count;
            for (int i = 0; i < n; i++)
            {
                if (_pendingActions[i] != null) return true;
            }
            return false;
        }
    }
}
