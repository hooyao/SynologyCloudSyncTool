using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace com.hy.synology.filemanager.core.file
{
    public class AsyncBinaryReader : BinaryReader
    {
        protected Stream _stream;

        public AsyncBinaryReader(Stream input) : base(input)
        {
            this._stream = input;
        }

        public AsyncBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            this._stream = input;
        }

        public AsyncBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            this._stream = input;
        }

        public async Task<byte> ReadByteAsync()
        {
            byte[] buffer = new byte[1];
            int result = await _stream.ReadAsync(buffer, 0, 1);
            int b = result == 0 ? -1 : buffer[0];
            //int b = await _stream.ReadByte()();
            if (b == -1)
            {
                throw new EndOfStreamException("End of file.");
            }

            return (byte) b;
        }

        public async Task<byte[]> ReadBytesAsync(int count)
        {
            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] result = new byte[count];
            int numRead = 0;
            do
            {
                int n = await _stream.ReadAsync(result, numRead, count);
                if (n == 0)
                {
                    break;
                }

                numRead += n;
                count -= n;
            } while (count > 0);

            if (numRead != result.Length)
            {
                // Trim array.  This should happen on EOF & possibly net streams.
                byte[] copy = new byte[numRead];
                Buffer.BlockCopy(result, 0, copy, 0, numRead);
                result = copy;
            }

            return result;
        }
    }
}