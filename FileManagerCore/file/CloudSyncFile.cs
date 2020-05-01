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

        public byte[] GetDecryptedContent(IDecryptor decryptor)
        {
            List<byte[]> decryptedDataBlockList = new List<byte[]>();
            byte[] buf = null;

            while (true)
            {
                if (buf != null)
                {
                    byte[] decryptedBlock = decryptor.DecryptBlock(buf, false);
                    decryptedDataBlockList.Add(decryptedBlock);
                }

                byte tag = this._binaryReader.ReadByte();
                if (tag != 0x42)
                {
                    break;
                }

                IFileStreamHandler<IDictionary<string, object>> dataHandler =
                    this._handlerFactory.GetHandler<IDictionary<string, object>>(tag);
                IDictionary<string, object> contentDirectory = dataHandler.Handle(this._binaryReader);
                contentDirectory.TryGetValue("type", out var typeValue);
                string typeValueString = typeValue as string;
                if (!"data".Equals(typeValueString))
                {
                    break;
                }

                buf = (byte[]) contentDirectory["data"];
            }

            return CryptoUtils.Concat(decryptedDataBlockList.ToArray());
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