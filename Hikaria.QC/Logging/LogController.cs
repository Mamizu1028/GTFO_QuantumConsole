using Hikaria.ES;
using Hikaria.QC.UI;
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
        private int _dataCountDelta = 0;
        private int _logCountDelta = 0;
        private bool _needScrollToLatest;
        private bool _isDirty = false;
        private bool _viewportSizeChanged = false;
        private bool _immediateScroll = false;
        private int _lastViewIndex = 0;
        private static bool _keepDataIndex = false;

        private float _tweenTime = 0.5f;
        private EnhancedScroller.TweenType _tweenType = EnhancedScroller.TweenType.easeOutSine;

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

        public void ProcessLogs()
        {
            if (DraggableUI.IsDraggingScroll || _scroller.IsScrolling || _scroller.IsTweening)
                return;

            int startDataIndex = _logDatas.Count == 0 ? 0 :
                    _keepDataIndex ? _lastViewIndex : _lastViewIndex = Math.Min(_scroller.StartDataIndex + 1, _logDatas.Count - 1);

            float scrollPosition = _scroller.ScrollPosition;

            ProcessQueuedLogActions();

            ReloadDataAndRebuildLayout();

            if (_logDatas.Count > 0)
            {
                if (_viewportSizeChanged || (scrollPosition > _scroller.ScrollSize))
                    _scroller.JumpToDataIndex(startDataIndex);
                else
                    _scroller.ScrollPosition = scrollPosition;
                _keepDataIndex = true;
                if (_needScrollToLatest)
                {
                    ScrollToLatestInternal(_immediateScroll);
                    _keepDataIndex = false;
                }
            }
            _isDirty = false;
            _viewportSizeChanged = false;
            _needScrollToLatest = false;
            _immediateScroll = false;
            _logCountDelta = 0;
            _dataCountDelta = 0;
        }

        public static void UserInteracted()
        {
            _keepDataIndex = false;
        }

        private void ProcessQueuedLogActions()
        {
            while (_logActionQueue.Count != 0)
            {
                (bool action, ILog? log) = _logActionQueue.Dequeue();
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
                _scroller.JumpToDataIndex(_logDatas.Count - 1,
                    tweenType: _tweenType,
                    tweenTime: _tweenTime);
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
                _dataCountDelta++;
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
                    _dataCountDelta--;
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
                _dataCountDelta--;
            }
        }

        public void Clear()
        {
            _lastViewIndex = 0;
            _keepDataIndex = false;
            _isDirty = false;
            _logActionQueue.Clear();
            _dataCountDelta = 0;
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