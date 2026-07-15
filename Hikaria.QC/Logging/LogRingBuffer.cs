using System;
using System.Collections;
using System.Collections.Generic;

namespace Hikaria.QC;

internal sealed class LogRingBuffer : IReadOnlyList<Log>
{
    private Log[] _buffer;
    private int _head;
    private int _count;

    public LogRingBuffer(int capacity)
    {
        if (capacity < 1)
            capacity = 1;

        _buffer = new Log[capacity];
    }

    public int Count => _count;
    internal int Capacity => _buffer.Length;

    public void SetCapacity(int capacity)
    {
        if (capacity < 1)
            capacity = 1;

        if (capacity != _buffer.Length)
            Resize(capacity);
    }

    public Log this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _buffer[PhysicalIndex(index)];
        }
    }

    public void AddLast(Log log)
    {
        if (_count == _buffer.Length)
            throw new InvalidOperationException("Buffer is full.");

        _buffer[PhysicalIndex(_count)] = log;
        _count++;
    }

    public Log RemoveFirst()
    {
        if (_count == 0)
            throw new InvalidOperationException("Buffer is empty.");

        Log log = _buffer[_head];
        _buffer[_head] = null!;
        _head = (_head + 1) % _buffer.Length;
        _count--;

        if (_count == 0)
            _head = 0;

        return log;
    }

    public bool Remove(Log log)
    {
        for (int i = 0; i < _count; i++)
        {
            if (ReferenceEquals(this[i], log))
            {
                RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public void Clear()
    {
        for (int i = 0; i < _count; i++)
            _buffer[PhysicalIndex(i)] = null!;

        _head = 0;
        _count = 0;
    }

    public IEnumerator<Log> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void RemoveAt(int index)
    {
        if (index < _count / 2)
        {
            for (int i = index; i > 0; i--)
                _buffer[PhysicalIndex(i)] = _buffer[PhysicalIndex(i - 1)];

            _buffer[_head] = null!;
            _head = (_head + 1) % _buffer.Length;
        }
        else
        {
            for (int i = index; i < _count - 1; i++)
                _buffer[PhysicalIndex(i)] = _buffer[PhysicalIndex(i + 1)];

            _buffer[PhysicalIndex(_count - 1)] = null!;
        }

        _count--;

        if (_count == 0)
            _head = 0;
    }

    private void Resize(int newCapacity)
    {
        if (newCapacity < _count)
            throw new ArgumentOutOfRangeException(nameof(newCapacity));

        Log[] newBuffer = new Log[newCapacity];
        for (int i = 0; i < _count; i++)
            newBuffer[i] = this[i];

        _buffer = newBuffer;
        _head = 0;
    }

    private int PhysicalIndex(int logicalIndex)
    {
        return (_head + logicalIndex) % _buffer.Length;
    }
}
