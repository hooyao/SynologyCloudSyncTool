using System;
using System.IO;

namespace com.hy.synology.filemanager.core.util
{
    public class CircularArrayQueue
    {
        private readonly int _baseCapacity;

        private byte[] _array = null;
        private int _start;
        private int _end;

        public CircularArrayQueue(int initCapacity)
        {
            if (initCapacity < 1)
            {
                throw new InvalidDataException();
            }

            _baseCapacity = initCapacity;
            _array = new byte[_baseCapacity + 1];
            _start = 0;
            _end = 0;
        }

        public bool Full => _start == (_end + 1) % _array.Length;

        public bool Empty => _end == _start;

        public int Size => _start > _end ? _array.Length - _start + _end : _end - _start;

        /// <summary>
        /// The capacity of the ring buffer, the capacity if always 1 less the the real array length used as buffer
        /// capacity == array.Length - 1, end position can't store any value
        /// </summary>
        public int Capacity => _array.Length - 1;

        private void Resize(int newCapacity)
        {
            if (newCapacity < this.Size)
            {
                throw new InvalidOperationException();
            }

            byte[] newArray = new byte[newCapacity + 1];
            int curSize = this.Size;
            byte[] curData = this.DeQueue(curSize);
            Buffer.BlockCopy(curData, 0, newArray, 0, curData.Length);
            _array = newArray;
            _start = 0;
            _end = curSize;
        }

        public void Shrink()
        {
            Resize(GetCapacity(Size));
        }

        public int GetCapacity(int newSize)
        {
            if (newSize <= _baseCapacity)
            {
                return _baseCapacity;
            }

            int log = (int) (Math.Ceiling(Math.Log2(((double) newSize) / _baseCapacity)) + 0.5);
            return _baseCapacity * (int) (Math.Pow(2, log) + 0.5);
        }

        public void EnQueue(byte[] data)
        {
            EnQueue(data, 0, data.Length);
        }

        public void EnQueue(byte[] source, int offset, int length)
        {
            if (length > this.Capacity - this.Size)
            {
                Resize(GetCapacity(length + this.Capacity));
            }

            int end2Tail = _array.Length - _end;
            if (length > end2Tail)
            {
                Buffer.BlockCopy(source, offset,
                    _array, _end,
                    end2Tail);
                Buffer.BlockCopy(source, offset + end2Tail,
                    _array, 0,
                    length - end2Tail);
            }
            else
            {
                Buffer.BlockCopy(source, offset, _array, _end, length);
            }

            _end = (_end + length) % _array.Length;
        }

        public byte[] DeQueue(int length)
        {
            byte[] buf = new byte[length];
            DeQueue(buf, 0, length);
            return buf;
        }

        public void DeQueue(byte[] dest, int offset, int length)
        {
            if (length <= 0)
            {
                return;
            }

            if (length > this.Size)
            {
                //TODO refactor exception
                throw new InvalidDataException();
            }

            int start2Tail = _array.Length - _start;
            if (start2Tail >= length)
            {
                Buffer.BlockCopy(_array, _start,
                    dest, offset,
                    length);
            }
            else
            {
                Buffer.BlockCopy(_array, _start,
                    dest, offset,
                    start2Tail);
                Buffer.BlockCopy(_array, 0,
                    dest, offset + start2Tail,
                    length - start2Tail);
            }

            _start = (_start + length) % _array.Length;
        }
    }
}