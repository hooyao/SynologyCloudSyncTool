using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace com.hy.synology.filemanager.core.crypto
{
    public class CryptoUtils
    {
        public static AsymmetricKeyParameter readPemPk(string pkContent)
        {
            AsymmetricCipherKeyPair keyPair;

            using (var reader = new StringReader(pkContent))
                keyPair = (AsymmetricCipherKeyPair) new PemReader(reader).ReadObject();

            return keyPair.Private;
        }

        public static byte[] Md5(byte[] input)
        {
            MD5Digest hash = new MD5Digest();
            hash.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[hash.GetDigestSize()];
            hash.DoFinal(result, 0);
            return result;
        }
        
        public static byte[] RsaOaepDeciper(byte[] data, AsymmetricKeyParameter privateKey)
        {
            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaEngine(), new Sha1Digest());
            cipher.Init(false, privateKey);

            return cipher.ProcessBlock(data, 0, data.Length);
        }

        public static string SaltedMd5(string salt, byte[] input)
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            MD5Digest hash = new MD5Digest();
            hash.BlockUpdate(saltBytes, 0, salt.Length);
            hash.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[hash.GetDigestSize()];
            hash.DoFinal(result, 0);
            return salt + Hex.ToHexString(result);
        }

        public static ParametersWithIV DeriveAESKeyParameters(byte[] passCode,
            byte[] salt)
        {
            int AES_KEY_SIZE_IN_BITS = 256;
            int AES_BLOCK_SIZE_IN_BYTES = 16;
            Tuple<byte[],byte[]> keyParameters= OpenSslKdf("md5", passCode, salt, AES_KEY_SIZE_IN_BITS / 8, AES_BLOCK_SIZE_IN_BYTES);
            KeyParameter keyParam = new KeyParameter(keyParameters.Item1);
            return new ParametersWithIV(keyParam, keyParameters.Item2);
        }
        
        public static byte[] DecryptByteArray(byte[] key, byte[] iv, byte[] encryptedData)
        {
            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            IBlockCipherPadding padding = new Pkcs7Padding();
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(blockCipher, padding);
            KeyParameter keyParam = new KeyParameter(key);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, iv.Length);

            //Decrypt            
            cipher.Init(false, keyParamWithIV);
            byte[] comparisonBytes = new byte[cipher.GetOutputSize(encryptedData.Length)];
            int length = cipher.ProcessBytes(encryptedData, comparisonBytes, 0);
            cipher.DoFinal(comparisonBytes, length); //Do the final block
            return comparisonBytes;
        }

        public static Tuple<byte[], byte[]> OpenSslKdf(
            string algo,
            byte[] passCode,
            byte[] salt,
            int keySize,
            int ivSize)
        {
            byte[] _salt = salt ?? Array.Empty<byte>();
            int iterCount = _salt.Length == 0 ? 1 : 1000;
            byte[] temp = Array.Empty<byte>();
            byte[] fd = Array.Empty<byte>();
            while (fd.Length < keySize + ivSize)
            {
                byte[] hashedCountTimes = Concat(temp, passCode, _salt);
                for (int i = 0; i < iterCount; i++)
                {
                    hashedCountTimes = Hasher(algo, hashedCountTimes);
                }

                temp = hashedCountTimes;
                fd = Concat(fd, temp);
            }

            byte[] key = fd[..keySize];
            byte[] iv = fd[keySize..(keySize + ivSize)];
            return Tuple.Create(key, iv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Concat(params byte[][] input)
        {
            byte[] bytes = new byte[input.Sum(a => a.Length)];
            int offset = 0;

            foreach (byte[] array in input)
            {
                Buffer.BlockCopy(array, 0, bytes, offset, array.Length);
                offset += array.Length;
            }

            return bytes;
        }

        // private static byte[] pkcs5s1(char[] passCode,
        //                                 byte[] salt,
        //                                 int iterCount) 
        // {
        //     Asn1Encodable algParams = PbeUtilities.GenerateAlgorithmParameters(
        //         "", salt, iterCount);
        //     ICipherParameters cipherParams = PbeUtilities.GenerateCipherParameters(
        //         "", passCode, algParams);
        //     return new byte[0];
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] Hasher(string algo, byte[] input)
        {
            return DigestUtilities.CalculateDigest(algo, input);
        }
    }
}