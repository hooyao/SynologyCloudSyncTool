using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.hy.synology.filemanager.connector.filesystem;
using com.hy.synology.filemanager.core.crypto;
using com.hy.synology.filemanager.core.util;

namespace com.hy.synology.filemanager.core.file
{
    public class CloudSyncFile : IDisposable
    {
        private FileItem _fileItem;
        private HandlerFactory _handlerFactory;

        private Stream _stream;
        private BinaryReader _binaryReader;

        private string _contentHash = null;

        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("__CLOUDSYNC_ENC__");

        public CloudSyncFile(FileItem fileItem, HandlerFactory handlerFactory)
        {
            _fileItem = fileItem;
            _handlerFactory = handlerFactory;
        }

        public void InitParsing()
        {
            //Read "__CLOUDSYNC_ENC__" 
            this._stream = this._fileItem.GetStream();
            this._binaryReader = new BinaryReader(this._stream);
            byte[] magicBytesRead = this._binaryReader.ReadBytes(MagicBytes.Length);
            if (!BytesUtils.ByteArrayCompare(MagicBytes, magicBytesRead))
            {
                throw new InvalidDataException($"File {this._fileItem.Name} is not Synology Cloud Sync file.");
            }

            //check "__CLOUDSYNC_ENC__" Md5 hash 
            byte[] magicHashRead = this._binaryReader.ReadBytes(32);
            byte[] magicHashComputed =
                Encoding.ASCII.GetBytes(BytesUtils.ByteArrayToLowerHexString(CryptoUtils.Md5(MagicBytes)));
            if (!BytesUtils.ByteArrayCompare(magicHashRead, magicHashComputed))
            {
                throw new InvalidDataException($"File {this._fileItem.Name} is not a valid Synology Cloud Sync file.");
            }
        }

        public FileMeta3 GetFileMeta()
        {
            byte metaTag = this._binaryReader.ReadByte();
            if (metaTag != 0x42)
            {
                throw new InvalidDataException($"File {this._fileItem.Name} fails to extract meta");
            }

            IFileStreamHandler<IDictionary<string, object>> metaHandler =
                this._handlerFactory.GetHandler<IDictionary<string, object>>(metaTag);
            IDictionary<string, object> metaDict = metaHandler.Handle(this._binaryReader);
            if (!(metaDict.ContainsKey("type") && "metadata".Equals(metaDict["type"])))
            {
                throw new InvalidDataException($"File {this._fileItem.Name} fails to extract meta");
            }

            return FileMeta3.fromDictionary(metaDict);
        }

        public IEnumerable<byte[]> GetDecryptedContent(IDecryptor decryptor)
        {
            byte[] buf = null;
            while (true)
            {
                if (buf != null)
                {
                    yield return decryptor.DecryptBlock(buf, false);
                }

                byte tag = this._binaryReader.ReadByte();
                if (tag != 0x42)
                {
                    yield break;
                }

                IFileStreamHandler<IDictionary<string, object>> dataHandler =
                    this._handlerFactory.GetHandler<IDictionary<string, object>>(tag);
                IDictionary<string, object> contentDirectory = dataHandler.Handle(this._binaryReader);
                contentDirectory.TryGetValue("type", out var typeValue);
                string typeValueString = typeValue as string;
                if (!"data".Equals(typeValueString))
                {
                    if ("metadata".Equals(typeValueString))
                    {
                        contentDirectory.TryGetValue("file_md5", out var md5Value);
                        string md5String = md5Value as string;
                        if (md5String != null)
                        {
                            this._contentHash = md5String;
                        }
                    }

                    yield break;
                }

                buf = (byte[]) contentDirectory["data"];
            }
        }

        public bool VerifyContentHash(byte[] computedHash)
        {
            if (this._contentHash == null)
            {
                byte tag = this._binaryReader.ReadByte();
                if (tag != 0x42)
                {
                    throw new InvalidDataException();
                }

                IFileStreamHandler<IDictionary<string, object>> dataHandler =
                    this._handlerFactory.GetHandler<IDictionary<string, object>>(tag);
                IDictionary<string, object> contentDirectory = dataHandler.Handle(this._binaryReader);
                contentDirectory.TryGetValue("type", out var typeValue);
                if ("metadata".Equals(typeValue))
                {
                    contentDirectory.TryGetValue("file_md5", out var md5Value);
                    string md5String = md5Value as string;
                    if (md5String == null)
                    {
                        throw new InvalidDataException();
                    }

                    this._contentHash = md5String;
                }
            }

            byte[] hashExpected = BytesUtils.HexStringToByteArray(this._contentHash);
            return BytesUtils.ByteArrayCompare(hashExpected, computedHash);
        }

        public void Dispose()
        {
            _binaryReader?.Close();
            _binaryReader?.Dispose();
            _stream?.Close();
            _stream?.Dispose();
        }
    }
}