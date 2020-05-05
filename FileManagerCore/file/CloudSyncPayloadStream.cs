using System;
using System.Collections.Generic;
using System.IO;
using com.hy.synology.filemanager.core.util;

namespace com.hy.synology.filemanager.core.file
{
    public class CloudSyncPayloadStream : Stream
    {
        private readonly IEnumerator<byte[]> _source;
        private readonly CircularArrayQueue _buf = new CircularArrayQueue(8 * 1024);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;

        public CloudSyncPayloadStream(IEnumerable<byte[]> source)
        {
            _source = source.GetEnumerator();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
            {
                throw new InvalidDataException();
            }

            while (_buf.Size < count)
            {
                if (!_source.MoveNext())
                {
                    break;
                }

                if (this._source.Current == null)
                {
                    break;
                }

                _buf.EnQueue(this._source.Current);
            }

            int readSize = _buf.Size <= count ? _buf.Size : count;
            byte[] bytesFromBuf = _buf.DeQueue(readSize);
            Buffer.BlockCopy(bytesFromBuf, 0, buffer, offset, readSize);
            return readSize;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            //Do nothing
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
    }
}