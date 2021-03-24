using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using WebAPI.Data;

namespace WebAPI.IO
{
    public class AzureStorageService
    {
        private ConcurrentDictionary<string, BlobServiceClient> _clients = new ConcurrentDictionary<string, BlobServiceClient>();

        public string InitblobService(string connectionString)
        {
            var newId = Guid.NewGuid().ToString();
            var blobServiceClient = new BlobServiceClient(connectionString);
            if (!this._clients.TryAdd(newId, blobServiceClient))
            {
                throw new Exception("Service Client exists, add failed.");
            }

            return newId;
        }

        public async Task<List<ItemDto>> ListItemsAsync(string serviceClientId, string containerName)
        {
            var result = new List<ItemDto>();
            if (this._clients.TryGetValue(serviceClientId, out var client))
            {
                var containerClient = client.GetBlobContainerClient(containerName);
                await foreach (var item in containerClient.GetBlobsAsync())
                {
                    if (!item.Deleted)
                    {
                        result.Add(new ItemDto()
                        {
                            Type = ItemType.File,
                            Path = item.Name
                        });
                    }
                }
            }

            return result;
        }
        
        public async Task Download(string serviceClientId, string containerName, string sourcePath, string destPath)
        {
            if (this._clients.TryGetValue(serviceClientId, out var client))
            {
                var containerClient = client.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(sourcePath);
                var download = await blobClient.DownloadAsync();

                await using (FileStream downloadFileStream = File.OpenWrite(destPath))
                {
                    await download.Value.Content.CopyToAsync(downloadFileStream);
                }
            }
        }
    }
}