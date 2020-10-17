using System.Collections.Generic;
using System.Threading.Tasks;
using SharedModel = Infrastructure.Shared.Model;

namespace BlazorClient.Services.Job
{
    public interface IJobService
    {
        Task<string> Test();

        Task<SharedModel.Job> CreateJob(string text);

        Task<IEnumerable<SharedModel.Job>> GetJobsUser();

        Task<SharedModel.Job> GetJobDetail(string id);
    }
}