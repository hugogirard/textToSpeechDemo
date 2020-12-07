using System.Threading.Tasks;

namespace BlazorClient.Services.Valet
{
    public interface IValetService
    {
        Task<string> GetSasFile(string blobname);
    }
}