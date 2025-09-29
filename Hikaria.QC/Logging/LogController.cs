using Hikaria.ES;
using System;
using System.Collections.Generic;

namespace Hikaria.QC
{
    internal class LogController : ILogController, IEnhancedScrollerDelegate
    {
        private readonly List<LogCellData> _logDatas = new List<LogCellData>(1025);
        private readonly Queue<(bool, ILog?)> _logActionQueue = new Queue<(bool, ILog?)>(1025);
        private EnhancedScroller _scroller;
        private LogCellView _logCellViewPrefab;

        private bool _calculateLayout;

        private int _logDataCountDelta = 0;
        private int _logCountDelta = 0;
        private int _logDataIndexOffset = 0;
        private float _scrollPositionOffset = 0;

        private bool _isDirty = false;
        private bool _viewportSizeChanged = false;
        private bool _needScrollToLatest;
        private bool _immediateScroll = false;
        private const float _immediateTweenTime = 0.1f;
        private float _tweenTime = 0.5f;
        private EnhancedScroller.TweenType _tweenType = EnhancedScroller.TweenType.easeOutSine;
        private bool _seamlessTween = true;
        private bool _firstFlush = true;

        public bool IsViewingLatestLog { get; private set; }

        public int MaxStoredLogs { get; set; }

        public IReadOnlyList<ILogData> LogDatas => _logDatas;

        public bool IsDirty => _isDirty;

        public LogController(EnhancedScroller scroller, LogCellView logCellViewPrefab, int maxStoredLogs = -1,
            EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.easeOutSine, float tweenTime = 0.5f, bool seamlessTween = true)
        {
            MaxStoredLogs = maxStoredLogs;
            _logCellViewPrefab = logCellViewPrefab;
            _scroller = scroller;
            _scroller.Delegate = this;
            _tweenTime = tweenTime;
            _tweenType = tweenType;
            _seamlessTween = seamlessTween;
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

            while (_logActionQueue.Count != 0)
            {
                var (action, log) = _logActionQueue.Dequeue();
                if (action)
                    AddLogInternal(log);
                else
                    RemoveLogInternal();
            }

            _scroller.ScrollPosition = 0;
            if (_viewportSizeChanged)
            {
                for (int i = 0; i < _logDatas.Count; i++)
                {
                    _logDatas[i].CellSize = 0;
                }
            }
            _calculateLayout = true;
            _scroller.ReloadData();
            _calculateLayout = false;
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
                    _scroller.JumpToDataIndex(_logDatas.Count - 1, 1f, 1f, false, _tweenType, _immediateScroll ? _immediateTweenTime : _tweenTime, 
                        forceCalculateRange: _logCountDelta != 0, seamlessTransition: _seamlessTween);
                }

                if (_logCountDelta == 0)
                {
                    _scroller.RefreshActive();
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

            if (_firstFlush)
            {
                if (_logDatas.Count > 0)
                {
                    _logDatas[0].CellSize = 0;
                }
                _isDirty = true;
                _firstFlush = false;
            }
        }

        public void AddLog(ILog log)
        {
            _isDirty = true;
            _logActionQueue.Enqueue(new(true, log));
        }

        public void RemoveLog()
        {
            if (_logDatas.Count == 0)
                return;

            _isDirty = true;
            _logActionQueue.Enqueue(new(false, null));
        }

        private void AddLogInternal(ILog log)
        {
            _logCountDelta++;
            if (log.NewLine || _logDatas.Count == 0)
            {
                _logDatas.Add(new LogCellData(log));
                _logDataCountDelta++;
            }
            else
            {
                _logDatas[^1].AppendLog(log);
            }

            if (MaxStoredLogs > 0)
            {
                while (_logDatas.Count > MaxStoredLogs)
                {
                    _logCountDelta -= _logDatas[0].Logs.Count;
                    _logDataCountDelta--;
                    _logDataIndexOffset--;
                    _scrollPositionOffset -= _logDatas[0].CellSize;
                    _logDatas.RemoveAt(0);
                }
            }
        }

        private void RemoveLogInternal()
        {
            if (_logDatas.Count == 0)
                return;

            var logData = _logDatas[^1];
            if (logData.RemoveLog())
            {
                _logCountDelta--;
            }
            if (logData.Logs.Count == 0)
            {
                _logDatas.RemoveAt(_logDatas.Count - 1);
                _logDataCountDelta--;
            }
        }

        public void Clear()
        {
            _isDirty = false;
            _logActionQueue.Clear();
            _logDataIndexOffset = 0;
            _logDataCountDelta = 0;
            _logCountDelta = 0;
            _scrollPositionOffset = 0;
            _scroller.ClearAll();
            _logDatas.Clear();
            _scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _logDatas.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _calculateLayout ? 0 : _logDatas[dataIndex].CellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            LogCellView cellView = scroller.GetCellView(_logCellViewPrefab) as LogCellView;
            cellView.SetData(_logDatas[dataIndex], _calculateLayout);
            return cellView;
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
    }
}