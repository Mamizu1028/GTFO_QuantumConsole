using System;
using System.Collections;
using System.Collections.Generic;

namespace Hikaria.QC
{
    /// <summary>
    /// 仅供 LogController 使用的环形双端队列。
    /// 头部移除（MaxStoredLogs 触发的裁剪）从原本 List.RemoveAt(0) 的 O(n)
    /// 降为 O(1)，同时保持 EnhancedScroller 需要的 O(1) 随机访问。
    /// </summary>
    /// <remarks>
    /// 容量上限通过 <see cref="Capacity"/> 设置：&lt;= 0 表示不限。
    /// 但本结构 <i>不</i> 在 AddLast 时自动裁剪——裁剪策略与对象池归还由 LogController 控制。
    /// </remarks>
    internal sealed class LogDataDeque : IReadOnlyList<ILogData>
    {
        private LogCellData[] _buffer;
        private int _head;
        private int _count;
        private int _capacityLimit; // <= 0 表示不限

        public LogDataDeque(int initialBufferSize = 1024)
        {
            if (initialBufferSize < 4) initialBufferSize = 4;
            _buffer = new LogCellData[initialBufferSize];
            _head = 0;
            _count = 0;
            _capacityLimit = -1;
        }

        public int Capacity
        {
            get => _capacityLimit;
            set => _capacityLimit = value;
        }

        public int Count => _count;

        public LogCellData this[int index]
        {
            get => _buffer[(_head + index) % _buffer.Length];
        }

        ILogData IReadOnlyList<ILogData>.this[int index] => this[index];

        public LogCellData Last
        {
            get => _buffer[(_head + _count - 1) % _buffer.Length];
        }

        public void AddLast(LogCellData item)
        {
            if (_count == _buffer.Length)
                Grow(_buffer.Length * 2);

            int idx = (_head + _count) % _buffer.Length;
            _buffer[idx] = item;
            _count++;
        }

        public LogCellData RemoveFirst()
        {
            if (_count == 0)
                throw new InvalidOperationException("Deque is empty.");

            var item = _buffer[_head];
            _buffer[_head] = null!;
            _head = (_head + 1) % _buffer.Length;
            _count--;
            return item;
        }

        public LogCellData RemoveLast()
        {
            if (_count == 0)
                throw new InvalidOperationException("Deque is empty.");

            int idx = (_head + _count - 1) % _buffer.Length;
            var item = _buffer[idx];
            _buffer[idx] = null!;
            _count--;
            return item;
        }

        public void Clear()
        {
            // 主动置 null 释放被引用对象，便于 GC
            if (_count > 0)
            {
                int len = _buffer.Length;
                for (int i = 0; i < _count; i++)
                {
                    _buffer[(_head + i) % len] = null!;
                }
            }
            _head = 0;
            _count = 0;
        }

        private void Grow(int newSize)
        {
            if (newSize < _count + 1) newSize = _count + 1;

            var newBuf = new LogCellData[newSize];
            int len = _buffer.Length;
            for (int i = 0; i < _count; i++)
            {
                newBuf[i] = _buffer[(_head + i) % len];
            }
            _buffer = newBuf;
            _head = 0;
        }

        public IEnumerator<ILogData> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
