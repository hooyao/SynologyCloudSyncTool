using System;
using System.IO;

namespace com.hy.synology.filemanager.core.crypto
{
    public class IntHandler : IFileStreamHandler<object>
    {
        public Type ReturnType => typeof(int);
        public byte SupportedTag => 0x01;

        public object Handle(BinaryReader br)
        {
            int? result;
            int len = br.ReadByte();
            if (len <= 0)
            {
                return null;
            }

            //multi-byte number encountered; guessing it is big-endian
            byte[] data = br.ReadBytes(len);
            switch (len)
            {
                case 1:
                    result = data[0];
                    break;
                case 2:
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(data);
                    result = BitConverter.ToInt16(data, 0);
                    break;
                case 4:
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(data);
                    result = BitConverter.ToInt32(data, 0);
                    break;
                default:
                    throw new InvalidDataException();
            }

            return result;
        }
    }
}