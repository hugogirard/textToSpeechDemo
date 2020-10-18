using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValetKey.Api.Service
{
    public class StorageService : IStorageService
    {
        private readonly string _containerName;
        private readonly BlobServiceClient _blobServiceClient;        
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        public StorageService(IConfiguration configuration)
        {
            _containerName = configuration["ContainerName"];
            _blobServiceClient = new BlobServiceClient(configuration["StorageConnectionString"]);            
            _storageSharedKeyCredential = new StorageSharedKeyCredential(_blobServiceClient.AccountName, configuration["StorageKey"]);
        }

        public string GetBlobDownloadLink(string blobname)
        {
            var blob = _blobServiceClient.GetBlobContainerClient(_containerName).GetBlobClient(blobname);

            var blobSasBuilder = new BlobSasBuilder

            {
                BlobContainerName = _containerName,
                BlobName = blobname,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sas = blobSasBuilder.ToSasQueryParameters(_storageSharedKeyCredential).ToString();

            return $"{blob.Uri}?{sas}";
        }
    }
}
