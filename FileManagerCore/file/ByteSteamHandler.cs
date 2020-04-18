using System;
using System.IO;

namespace com.hy.synology.filemanager.core.crypto
{
    public class ByteSteamHandler : IFileStreamHandler<byte[]>
    {
        public Type ReturnType => typeof(byte[]);
        public byte SupportedTag => 0x11;

        public byte[] Handle(BinaryReader br)
        {
            byte[] lenData = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenData);
            int len = BitConverter.ToInt16(lenData, 0);
            return br.ReadBytes(len);
        }
    }
}