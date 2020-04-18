using System.IO;
using com.hy.synology.filemanager.core.crypto;
using FileManagerCore.file;

namespace com.hy.synology.filemanager.core.file
{
    public class CloudSyncEncryptedItemStream
    {
        private BinaryReader _binaryReader;
        private HandlerFactory _handlerFactory;
            
        public CloudSyncEncryptedItemStream(Stream stream, HandlerFactory handlerFactory)
        {
            this._binaryReader = new BinaryReader(stream);
            this._handlerFactory = handlerFactory;
        }

        public IFileMeta GetFileMeta()
        {
            return null;
        }
        
    }
}