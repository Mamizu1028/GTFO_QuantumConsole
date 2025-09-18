using Hikaria.ES;
using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;

namespace Hikaria.QC
{
    public class LogController : ILogController, IEnhancedScrollerDelegate
    {
        private readonly List<ILog> _consoleLogs = new List<ILog>(1025);
        private readonly List<LogCellData> _logDatas = new List<LogCellData>(1025);
        private EnhancedScroller _scroller;
        private LogCellView _logCellViewPrefab;
        private bool _calculateLayout;

        public int MaxStoredLogs { get; set; }
        public IReadOnlyList<ILog> Logs => _consoleLogs;

        [HideFromIl2Cpp]
        public void Setup(EnhancedScroller scroller, LogCellView logCellViewPrefab, int maxStoredLogs = -1)
        {
            MaxStoredLogs = maxStoredLogs;
            _logCellViewPrefab = logCellViewPrefab;
            _scroller = scroller;
            _scroller.Delegate = this;
        }

        public void AddLog(ILog log)
        {
            _consoleLogs.Add(log);

            if (!log.NewLine && _logDatas.Count > 0)
            {
                _logDatas[_logDatas.Count - 1].LogText += log.Text;
            }
            else
            {
                _logDatas.Add(new LogCellData()
                {
                    LogText = log.Text
                });
            }

            if (MaxStoredLogs > 0)
            {
                while (_consoleLogs.Count > MaxStoredLogs)
                {
                    _consoleLogs.RemoveAt(0);
                }
                while (_logDatas.Count > MaxStoredLogs)
                {
                    _logDatas.RemoveAt(0);
                }
            }
        }

        public void RemoveLog()
        {
            if (_consoleLogs.Count > 0)
            {
                ILog log = _consoleLogs[_consoleLogs.Count - 1];
                _consoleLogs.RemoveAt(_consoleLogs.Count - 1);

                int removeLength = log.Text.Length;
                if (log.NewLine && _consoleLogs.Count > 0)
                {
                    removeLength += Environment.NewLine.Length;
                }

                var logData = _logDatas[_logDatas.Count - 1];
                logData.LogText = logData.LogText.Remove(logData.LogText.Length - removeLength, removeLength);
            }
        }

        public void Clear()
        {
            _consoleLogs.Clear();
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
            _scroller.ScrollPosition = 0;

            for (int i = 0; i < _logDatas.Count; i++)
            {
                _logDatas[i].CellSize = 0;
            }

            _calculateLayout = true;
            _scroller.ReloadData();
            _calculateLayout = false;
            _scroller.ReloadData();

            _scroller.JumpToDataIndex(Math.Max(0, _logDatas.Count - 1));
        }

        public void ScrollConsoleToLatest()
        {
            _scroller.JumpToDataIndex(Math.Max(0, _logDatas.Count - 1));
        }
    }
}