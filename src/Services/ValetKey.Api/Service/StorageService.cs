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
        private readonly BlobContainerClient _container;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        public StorageService(IConfiguration configuration)
        {
            _containerName = configuration["ContainerName"];
            _container = new BlobContainerClient(configuration["StorageConnectionString"], _containerName);
            _storageSharedKeyCredential = new StorageSharedKeyCredential(configuration["AccountName"], configuration["StorageKey"]);
        }

        public string GetBlobDownloadLink(string blobname)
        {
            var blob = _container.GetBlobClient(blobname);

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
