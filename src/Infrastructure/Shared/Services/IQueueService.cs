using System.Threading.Tasks;

namespace Shared.Services
{
    public interface IQueueService
    {
        Task SendMessageAsync<T>(T entity) where T : class;
    }
}