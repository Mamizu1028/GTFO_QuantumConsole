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
        private bool _needScrollToLatest;
        private bool _isDirty = false;
        private bool _viewportSizeChanged = false;
        private bool _immediateScroll = false;

        private float _tweenTime = 0.5f;
        private EnhancedScroller.TweenType _tweenType = EnhancedScroller.TweenType.easeOutSine;

        public bool IsViewingLatestLog { get; private set; }

        public int MaxStoredLogs { get; set; }

        public IReadOnlyList<ILogData> LogDatas => _logDatas;

        public bool IsDirty => _isDirty;

        public LogController(EnhancedScroller scroller, LogCellView logCellViewPrefab, int maxStoredLogs = -1, EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.easeOutSine, float tweenTime = 0.5f)
        {
            MaxStoredLogs = maxStoredLogs;
            _logCellViewPrefab = logCellViewPrefab;
            _scroller = scroller;
            _scroller.Delegate = this;
            _tweenTime = tweenTime;
            _tweenType = tweenType;
        }

        private bool _wasTweening = false;
        private bool _pendingProcessAfterTween = false;
        public void ProcessLogs()
        {
            bool isTweening = _scroller.IsTweening;
            if (_wasTweening && !isTweening)
            {
                _pendingProcessAfterTween = true;
                _wasTweening = false;
                return;
            }
            if (isTweening)
            {
                _wasTweening = true;
                return;
            }
            if (_pendingProcessAfterTween)
            {
                _pendingProcessAfterTween = false;
                return;
            }

            IsViewingLatestLog = _logDatas.Count == 0 || _scroller.EndDataIndex == _logDatas.Count - 1;

            int startDataIndex = _logDatas.Count == 0 ? 0 : _scroller.StartDataIndex + 1;
            float scrollPosition = _scroller.ScrollPosition;
            float linearVelocity = _scroller.LinearVelocity;
            _needScrollToLatest |= IsViewingLatestLog;

            ProcessQueuedLogActions();

            ReloadDataAndRebuildLayout();

            if (_logDatas.Count > 0)
            {
                startDataIndex = Math.Max(0, startDataIndex + _logDataIndexOffset);
                if (_viewportSizeChanged)
                {
                    _scroller.JumpToDataIndex(startDataIndex, 0f, 1f, false);
                }
                else
                {
                    _scroller.ScrollPosition = scrollPosition;
                    _scroller.LinearVelocity = linearVelocity;
                }

                if (_needScrollToLatest)
                {
                    ScrollToLatestInternal(_immediateScroll);
                }
            }
            _isDirty = false;
            _viewportSizeChanged = false;
            _needScrollToLatest = false;
            _immediateScroll = false;
            _logDataIndexOffset = 0;
            _logCountDelta = 0;
            _logDataCountDelta = 0;
        }

        private void ProcessQueuedLogActions()
        {
            while (_logActionQueue.Count != 0)
            {
                var (action, log) = _logActionQueue.Dequeue();
                if (action)
                    AddLogInternal(log);
                else
                    RemoveLogInternal();
            }
        }

        private void ReloadDataAndRebuildLayout()
        {
            _scroller.ScrollPosition = 0;
            for (int i = 0; i < _logDatas.Count; i++)
            {
                _logDatas[i].CellSize = 0;
            }
            _calculateLayout = true;
            _scroller.ReloadData();
            _calculateLayout = false;
            _scroller.ReloadData();
        }

        private void ScrollToLatestInternal(bool immediate)
        {
            if (_logDatas.Count == 0)
            {
                _scroller.ScrollPosition = 0;
                return;
            }

            if (immediate)
            {
                _scroller.ScrollPosition = _scroller.ScrollSize;
            }
            else
            {
                _scroller.JumpToDataIndex(_logDatas.Count - 1, 1f, 1f, false, _tweenType, _tweenTime);
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
                _logDatas[_logDatas.Count - 1].AppendLog(log);
            }

            if (MaxStoredLogs > 0)
            {
                while (_logDatas.Count > MaxStoredLogs)
                {
                    _logCountDelta -= _logDatas[0].Logs.Count;
                    _logDataCountDelta--;
                    _logDataIndexOffset--;
                    _logDatas.RemoveAt(0);
                }
            }
        }

        private void RemoveLogInternal()
        {
            var logDataIndex = _logDatas.Count - 1;
            var logData = _logDatas[logDataIndex];
            if (logData.RemoveLog())
            {
                _logCountDelta--;
            }
            if (logData.Logs.Count == 0)
            {
                _logDatas.RemoveAt(logDataIndex);
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
            return _logDatas[dataIndex].CellSize;
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