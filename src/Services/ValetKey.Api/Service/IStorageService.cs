namespace ValetKey.Api.Service
{
    public interface IStorageService
    {
        string GetBlobDownloadLink(string blobname);
    }
}