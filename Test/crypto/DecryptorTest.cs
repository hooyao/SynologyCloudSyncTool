using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using com.hy.synology.filemanager.core.crypto;
using com.hy.synology.filemanager.core.file;
using com.hy.synology.filemanager.core.util;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

namespace com.hy.synology.filemanager.test.crypto
{
    public class DecrypterTest
    {
        private string privateKey;
        private string PublicKey;

        [Test]
        public void test()
        {
            this.ReadKeyFile();
            this.ParseFile();
        }

        private void ReadKeyFile()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = $"{directory}\\Resources\\crypto\\key.zip";
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.Equals("private.pem"))
                    {
                        using (Stream stream = entry.Open())
                        using (MemoryStream sr = new MemoryStream())
                        {
                            stream.CopyTo(sr);
                            this.privateKey = Encoding.ASCII.GetString(sr.ToArray());
                        }
                    }

                    if (entry.Name.Equals("public.pem"))
                    {
                        using (Stream stream = entry.Open())
                        using (MemoryStream sr = new MemoryStream())
                        {
                            stream.CopyTo(sr);
                            this.PublicKey = Encoding.ASCII.GetString(sr.ToArray());
                        }
                    }
                }
            }
        }

        private void ParseFile()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = $"{directory}\\Resources\\crypto\\encrypted_jpg_01.jpg";
            string magic = "__CLOUDSYNC_ENC__";
            byte[] magicBytes = Encoding.ASCII.GetBytes(magic);

            using FileStream fs = new FileStream(path, FileMode.Open);
            using BinaryReader br = new BinaryReader(fs);

            byte[] value = br.ReadBytes(magicBytes.Length);
            Console.WriteLine(ByteArrayCompare(magicBytes, value));

            //check "__CLOUDSYNC_ENC__" Md5 hash 
            value = br.ReadBytes(32);
            byte[] expectedMagicHash =
                Encoding.ASCII.GetBytes(BytesUtils.ByteArrayToLowerHexString(Md5(magicBytes)));
            Assert.AreEqual(expectedMagicHash, value);

            //
            HandlerFactory handlerFactory = new HandlerFactory();
            StringHandler stringHandler = new StringHandler();
            IntHandler intHandler = new IntHandler();
            ByteSteamHandler byteSteamHandler = new ByteSteamHandler();
            OrderedDictHandler dictHandler = new OrderedDictHandler(handlerFactory);
            handlerFactory.AddHandler(stringHandler);
            handlerFactory.AddHandler(dictHandler);
            handlerFactory.AddHandler(intHandler);
            handlerFactory.AddHandler(byteSteamHandler);

            byte metaTag = br.ReadByte();
            if (metaTag != 0x42)
            {
                throw new InvalidDataException();
            }

            IFileStreamHandler<IDictionary<string, object>> metaHandler =
                handlerFactory.GetHandler<IDictionary<string, object>>(metaTag);
            IDictionary<string, object> metaDict = metaHandler.Handle(br);
            if (!(metaDict.ContainsKey("type") && "metadata".Equals(metaDict["type"])))
            {
                throw new InvalidDataException();
            }

            FileMeta3 fileMeta = FileMeta3.fromDictionary(metaDict);

            Console.WriteLine(fileMeta);

            AsymmetricKeyParameter akp = CryptoUtils.readPemPk(this.privateKey);
            byte[] sessionKeyCharArray = CryptoUtils.RsaOaepDeciper(fileMeta.EncKey2, akp);

            string computedSessionKeyHash = CryptoUtils.SaltedMd5(
                fileMeta.SessionKeyHash.Substring(0, 10), sessionKeyCharArray);

            if (!fileMeta.SessionKeyHash.Equals(computedSessionKeyHash))
            {
                throw new InvalidDataException("key is incorrect");
            }

            //decrypt content
            byte[] sessionKey = BytesUtils.HexStringToByteArray(
                Encoding.ASCII.GetString(sessionKeyCharArray));
            Console.Write(sessionKey);
            ParametersWithIV keys =
                CryptoUtils.DeriveAESKeyParameters(sessionKey, null);
            AesCbcCryptor decryptor =
                new AesCbcCryptor(((KeyParameter) keys.Parameters).GetKey(), keys.GetIV());
            List<byte[]> decryptedData = new List<byte[]>();
            byte[] buf = null;
            byte[] decBuf = null;
            IDictionary<string, object> dataResult = null;
            while (true)
            {
                byte dataTag = br.ReadByte();
                if (dataTag == 0x40)
                {
                    Console.WriteLine("come here");
                }

                if (dataTag != 0x42)
                {
                    decBuf = decryptor.DecryptBlock(buf, true);
                    decryptedData.Add(decBuf);
                    break;
                }

                if (buf != null)
                {
                    decBuf = decryptor.DecryptBlock(buf, false);
                    decryptedData.Add(decBuf);
                }

                IFileStreamHandler<IDictionary<string, object>> dataHandler =
                    handlerFactory.GetHandler<IDictionary<string, object>>(metaTag);
                dataResult = dataHandler.Handle(br);
                object typeValue=null;
                dataResult.TryGetValue("type", out typeValue);
                string typeValueString = typeValue as string;
                if (!"data".Equals(typeValueString))
                {
                    break;
                }

                buf = (byte[]) dataResult["data"];

            }

            byte[] decData = CryptoUtils.Concat(decryptedData.ToArray());
            File.WriteAllBytes("z:\\123.jpg.lz4", decData);

            //last directory
            
            //TODO validate dataResult["type"] == "metadata"
            string fileMd5 = (string)dataResult["file_md5"];
            
            Console.Write(fileMd5);

        }

        static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        static byte[] Md5(byte[] input)
        {
            MD5Digest hash = new MD5Digest();
            hash.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[hash.GetDigestSize()];
            hash.DoFinal(result, 0);
            return result;
        }


        [Test]
        public void testOpenSslKdf()
        {
            byte[] passCode = Encoding.ASCII.GetBytes("test");
            byte[] salt = BytesUtils.HexStringToByteArray("F6818CAE131872BD");
            Tuple<byte[], byte[]> keyParameters = CryptoUtils.OpenSslKdf("md5", passCode, null, 32, 16);
            Console.WriteLine(BytesUtils.ByteArrayToLowerHexString(keyParameters.Item1));
            Console.WriteLine(BytesUtils.ByteArrayToLowerHexString(keyParameters.Item2));

            byte[] encrypted = BytesUtils.HexStringToByteArray(
                "7faeb63f28f0ae4cd1eb5a92447c8111591755ded24d4a55ea8ec8ac9b3e9bc80dd616aa981b4cccbbeadbf1a3d77e2b");
            byte[] password = BytesUtils.HexStringToByteArray("62754a78392f79396656");
            salt = new byte[0];
            Tuple<byte[], byte[]> tmp = CryptoUtils.OpenSslKdf("md5", password, salt, 32, 16);
            byte[] plain = CryptoUtils.DecryptByteArray(tmp.Item1, tmp.Item2, encrypted);
            Console.WriteLine(BytesUtils.ByteArrayToLowerHexString(plain));
            Console.WriteLine("42785932412d6f75527049385952766d69576969354b6b4346334c564e314f36");
        }
    }
}