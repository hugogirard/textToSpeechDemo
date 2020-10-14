using System.Threading.Tasks;

namespace BlazorClient.Services.Job
{
    public interface IJobService
    {
        Task<string> Test();

        Task<string> CreateJob(string text);
    }
}