using Hikaria.ES;
using System;
using System.Collections.Generic;

namespace Hikaria.QC
{
    internal class LogController : ILogController, IEnhancedScrollerDelegate
    {
        private readonly List<LogCellData> _logDatas = new List<LogCellData>(1025);
        private EnhancedScroller _scroller;
        private LogCellView _logCellViewPrefab;
        private bool _calculateLayout;

        public int MaxStoredLogs { get; set; }

        public IReadOnlyList<ILogData> LogDatas => _logDatas;

        public LogController(EnhancedScroller scroller, LogCellView logCellViewPrefab, int maxStoredLogs = -1)
        {
            MaxStoredLogs = maxStoredLogs;
            _logCellViewPrefab = logCellViewPrefab;
            _scroller = scroller;
            _scroller.Delegate = this;
        }

        public void AddLog(ILog log)
        {
            if (log.NewLine || _logDatas.Count == 0)
                _logDatas.Add(new LogCellData(log));
            else
                _logDatas[_logDatas.Count - 1].AppendLog(log);

            if (MaxStoredLogs > 0)
            {
                while (_logDatas.Count > MaxStoredLogs)
                {
                    _logDatas.RemoveAt(0);
                }
            }
        }

        public void RemoveLog()
        {
            if (_logDatas.Count == 0)
                return;

            var logDataIndex = _logDatas.Count - 1;
            var logData = _logDatas[logDataIndex];
            if (!logData.RemoveLog() || logData.Logs.Count == 0)
                _logDatas.RemoveAt(logDataIndex);
        }

        public void Clear()
        {
            _scroller.ClearAll();
            _scroller.ScrollPosition = 0;
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
            var logData = _logDatas[dataIndex];
            cellView.SetData(logData, _calculateLayout);
            return cellView;
        }

        public void FlushLogText()
        {
            bool hasLogData = _logDatas.Count > 0;

            _scroller.ScrollPosition = 0;

            for (int i = 0; i < _logDatas.Count; i++)
            {
                _logDatas[i].CellSize = 0;
            }

            _calculateLayout = true;
            _scroller.ReloadData();
            _calculateLayout = false;
            _scroller.ReloadData();

            if (hasLogData)
            {
                _scroller.JumpToDataIndex(Math.Max(0, _logDatas.Count - 1));
            }
            else
            {
                _scroller.ScrollPosition = 0;
            }
        }

        public void ScrollConsoleToLatest()
        {
            _scroller.ScrollPosition = _scroller.ScrollSize;
        }
    }
}