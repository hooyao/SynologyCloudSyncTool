using Akka.Actor;

namespace WebAPI.Data
{
    public enum ConnectorType
    {
        SOURCE_AZURE_BLOB,
        DEST_AZURE_BLOB
    }

    public class ConnectorDto
    {
        public string Id { get; set; }
        public ConnectorType Type { get; set; }
        public string ConnString { get; set; }
        public string ContainerName { get; set; }
    }
}