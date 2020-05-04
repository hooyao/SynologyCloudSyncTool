using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.hy.synology.filemanager.core.crypto;

namespace com.hy.synology.filemanager.core.file
{
    public class CloudSyncPayloadStream : Stream
    {
        private readonly IEnumerator<byte[]> _source;
        private readonly Queue<byte> _buf = new Queue<byte>();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;

        /// <summary>
        /// Creates a new instance of <code>EnumerableStream</code>
        /// </summary>
        /// <param name="source">The source enumerable for the EnumerableStream</param>
        /// <param name="serializer">A function that converts an instance of <code>T</code> to IEnumerable<byte></param>
        public CloudSyncPayloadStream(IEnumerable<byte[]> source)
        {
            _source = source.GetEnumerator();
        }

        private bool SerializeNext()
        {
            if (!_source.MoveNext())
                return false;
            if (this._source.Current == null)
            {
                return false;
            }

            foreach (var b in _source.Current)
                _buf.Enqueue(b);

            return true;
        }

        private byte? NextByte()
        {
            if (_buf.Any() || SerializeNext())
            {
                return _buf.Dequeue();
            }

            return null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            while (read < count)
            {
                var mayb = NextByte();
                if (mayb == null) break;

                buffer[offset + read] = (byte) mayb;
                read++;
            }

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
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