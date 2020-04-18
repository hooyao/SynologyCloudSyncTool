using System;
using System.IO;
using System.Linq;
using K4os.Compression.LZ4.Streams;

namespace com.hy.synology.filemanager.core.util
{
    public static class BytesUtils
    {
        private static readonly uint[] Lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s=i.ToString("X2").ToLower();
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        public static string ByteArrayToUpperHexString(byte[] bytes)
        {
            return ByteArrayToLowerHexString(bytes).ToUpper();
        }
        
        public static string ByteArrayToLowerHexString(byte[] bytes)
        {
            char[] result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = Lookup32[bytes[i]];
                result[2*i] = (char)val;
                result[2*i + 1] = (char) (val >> 16);
            }
            return new string(result);
        }
        
        public static byte[] HexStringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
        
        public static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        //TODO refactor this method to task stream and find a proper place @HUYAO
        public static byte[] UnLz4(byte[] compressedBytes)
        {
            using (var source = LZ4Stream.Decode(new MemoryStream(compressedBytes)))
            using (var target = new MemoryStream())
            {
                source.CopyTo(target);
                return target.ToArray();
            }
        }
    }
}