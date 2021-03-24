using System;
using System.IO;
using System.Text;

namespace com.hy.synology.filemanager.core.crypto
{
    public class StringHandler : IFileStreamHandler<string>
    {
        public StringHandler()
        {
        }

        public Type ReturnType => typeof(string);
        public byte SupportedTag => 0x10;

        public string Handle(BinaryReader br)
        {
            byte[] lenData = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenData);
            int len = BitConverter.ToInt16(lenData, 0);
            byte[] raw = br.ReadBytes(len);
            return Encoding.UTF8.GetString(raw);
        }
    }
}