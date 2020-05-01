using System;
using System.Collections.Generic;
using System.IO;

namespace com.hy.synology.filemanager.connector.filesystem
{
    public class FileItem
    {
        private string _filePath;
        public string Name => Path.GetFileName(this._filePath);
        public string ParentPath => this._filePath.Substring(0, this._filePath.Length - Name.Length);

        public FileItem(string filePath)
        {
            this._filePath = filePath;
        }

        public Stream GetStream()
        {
            return new FileStream(this._filePath, FileMode.Open);
        }
    }

    public class FileSystemConnector
    {
        public IEnumerable<int> ListItems(string directoryPath)
        {
            throw new NotSupportedException();
        }
    }
}