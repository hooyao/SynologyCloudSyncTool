using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using com.hy.synology.filemanager.connector.filesystem;
using com.hy.synology.filemanager.core.crypto;
using com.hy.synology.filemanager.core.file;
using com.hy.synology.filemanager.core.util;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace com.hy.synolocgy.filemanager.simplecli
{
    class Program
    {
        private static readonly string CurrentDirectory =
            Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        private static readonly string KeyFilePath = Path.Join(CurrentDirectory, "key.zip");
        private static readonly string OutputDirectory = Path.Join(CurrentDirectory, "output");

        public static void Main(string[] args)
        {
            if (!args.Any()) return;
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            //init beans
            HandlerFactory handlerFactory = new HandlerFactory();
            StringHandler stringHandler = new StringHandler();
            IntHandler intHandler = new IntHandler();
            ByteSteamHandler byteSteamHandler = new ByteSteamHandler();
            OrderedDictHandler dictHandler = new OrderedDictHandler(handlerFactory);
            handlerFactory.AddHandler(stringHandler);
            handlerFactory.AddHandler(dictHandler);
            handlerFactory.AddHandler(intHandler);
            handlerFactory.AddHandler(byteSteamHandler);


            var keyReader = new CloudSyncKeyReader();
            AsymmetricCipherKeyPair keyPair = keyReader.GetKeyPair(KeyFilePath);
            foreach (var t in args)
            {
                try
                {
                    FileItem fi = new FileItem(t);
                    using (CloudSyncFile cloudSyncFile = new CloudSyncFile(fi, handlerFactory))
                    {
                        cloudSyncFile.InitParsing();
                        FileMeta3 fileMeta = cloudSyncFile.GetFileMeta();

                        //Generate session key and make sure it matches the file
                        byte[] sessionKeyComputed = CryptoUtils.RsaOaepDeciper(fileMeta.EncKey2, keyPair.Private);
                        string sessionKeyHashStrComputed = CryptoUtils.SaltedMd5(
                            fileMeta.SessionKeyHash.Substring(0, 10), sessionKeyComputed);

                        if (!fileMeta.SessionKeyHash.Equals(sessionKeyHashStrComputed))
                        {
                            throw new InvalidDataException($"File {fi.Name}, Computed session key is incorrect.");
                        }

                        //decrypt content
                        byte[] sessionKeyBytes = BytesUtils.HexStringToByteArray(
                            Encoding.ASCII.GetString(sessionKeyComputed));
                        ParametersWithIV keys =
                            CryptoUtils.DeriveAESKeyParameters(sessionKeyBytes, null);
                        AesCbcCryptor decryptor =
                            new AesCbcCryptor(((KeyParameter) keys.Parameters).GetKey(), keys.GetIV());

                        byte[] decryptedContent = cloudSyncFile.GetDecryptedContent(decryptor);
                        byte[] decompressedContent = BytesUtils.UnLz4(decryptedContent);
                        File.WriteAllBytes(Path.Join(OutputDirectory,fileMeta.FileName),decompressedContent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Decrypt {t} failed. Error: {ex.Message}");
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}