using System.Collections.Generic;

namespace com.hy.synology.filemanager.connector.common
{
    public interface IConnector
    {
        IEnumerable<int> ListItems(string directoryPath);
        
    }
}