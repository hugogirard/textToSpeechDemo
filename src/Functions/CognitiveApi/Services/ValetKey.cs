using Azure.Storage;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveApi
{
    public class ValetKey
    {

        public string GetSharedAccessReferenceView(string blobName,
                                                    string accountName,
                                                    string containerName)
        {           
            var storageSharedKeyCredential = new StorageSharedKeyCredential(accountName, Environment.GetEnvironmentVariable("AudioFilesStorageKey"));

            var blobSasBuilder = new BlobSasBuilder

            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();            
        }
    }
}
