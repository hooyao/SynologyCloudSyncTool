namespace WebAPI.Data
{
    public class DownloadJob
    {
        public string BlobId { get; set; }
        public string ContainerName { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
    }
}