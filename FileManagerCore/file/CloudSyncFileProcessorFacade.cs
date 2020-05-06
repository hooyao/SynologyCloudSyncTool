﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using com.hy.synology.filemanager.connector.filesystem;
using com.hy.synology.filemanager.core.crypto;
using com.hy.synology.filemanager.core.exception;
using com.hy.synology.filemanager.core.util;
using K4os.Compression.LZ4.Streams;
using Org.BouncyCastle.Crypto.Parameters;

namespace com.hy.synology.filemanager.core.file
{
    public class CloudSyncFileProcessorFacade
    {
        private readonly HandlerFactory _handlerFactory;
        private readonly CloudSyncKey _cloudSyncKey;
        private readonly IExceptionHandler _exceptionHandler;

        public CloudSyncFileProcessorFacade(HandlerFactory handlerFactory,
            CloudSyncKey cloudSyncKey, IExceptionHandler exceptionHandler)
        {
            _handlerFactory = handlerFactory;
            _cloudSyncKey = cloudSyncKey;
            _exceptionHandler = exceptionHandler;
        }

        public bool ProcessFile(string sourcePath, string destDir, bool respectFileNameInMeta = true)
        {
            string destPath = null;
            try
            {
                FileItem fi = new FileItem(sourcePath);
                using (CloudSyncFile cloudSyncFile = new CloudSyncFile(fi, _handlerFactory))
                {
                    cloudSyncFile.InitParsing();
                    FileMeta3 fileMeta = cloudSyncFile.GetFileMeta();

                    //Generate session key and make sure it matches the file
                    byte[] sessionKeyComputed =
                        CryptoUtils.RsaOaepDeciper(fileMeta.EncKey2, this._cloudSyncKey.KeyPair.Private);
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

                    destPath = Path.Join(destDir,
                        respectFileNameInMeta ? fileMeta.FileName : Path.GetFileName(sourcePath));

                    var hasher = MD5.Create();
                    using (CloudSyncPayloadStream cspls =
                        new CloudSyncPayloadStream(cloudSyncFile.GetDecryptedContent(decryptor)))
                    using (LZ4DecoderStream lz4ds = LZ4Stream.Decode(cspls))
                    using (CryptoStream md5HashStream = new CryptoStream(lz4ds, hasher, CryptoStreamMode.Read))
                    using (FileStream writeFs = File.OpenWrite(destPath))
                    using (BufferedStream writeBs = new BufferedStream(writeFs))
                    {
                        md5HashStream.CopyTo(writeBs);
                    }

                    if (!cloudSyncFile.VerifyContentHash(hasher.Hash))
                    {
                        throw new InvalidDataException();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }

                this._exceptionHandler.Handle(ex);
            }

            return false;
        }
    }
}