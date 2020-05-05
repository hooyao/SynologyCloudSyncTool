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
            if (data.Length > this.Capacity - this.Size)
            {
                Resize(GetCapacity(data.Length + this.Capacity));
            }

            int end2Tail = _array.Length - _end;
            if (data.Length > end2Tail)
            {
                Buffer.BlockCopy(data, 0,
                    _array, _end,
                    end2Tail);
                Buffer.BlockCopy(data, end2Tail,
                    _array, 0,
                    data.Length - end2Tail);
            }
            else
            {
                Buffer.BlockCopy(data, 0, _array, _end, data.Length);
            }

            _end = (_end + data.Length) % _array.Length;
        }

        public byte[] DeQueue(int length)
        {
            if (length <= 0)
            {
                return new byte[0];
            }

            if (length > this.Size)
            {
                //TODO refactor exception
                throw new InvalidDataException();
            }

            byte[] result = new byte[length];
            int start2Tail = _array.Length - _start;
            if (start2Tail >= length)
            {
                Buffer.BlockCopy(_array, _start,
                    result, 0,
                    length);
            }
            else
            {
                Buffer.BlockCopy(_array, _start,
                    result, 0,
                    start2Tail);
                Buffer.BlockCopy(_array, 0,
                    result, start2Tail,
                    length - start2Tail);
            }

            _start = (_start + length) % _array.Length;
            return result;
        }
    }
}