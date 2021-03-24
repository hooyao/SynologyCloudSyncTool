using Akka.Actor;
using WebAPI.Data;
using WebAPI.IO;

namespace WebAPI.Actors
{
    public class AzureDownloadActor:ReceiveActor
    {
        private readonly AzureStorageService _azureStorageService;

        public AzureDownloadActor(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
            ReceiveAsync<DownloadJob>(async job =>
            {
                await this._azureStorageService.Download(job.BlobId, job.ContainerName, job.SourcePath, job.TargetPath);
            });
        }
    }
}