using System.Threading.Tasks;

namespace Infrastructure.Shared.Services
{
    public interface IQueueService
    {
        Task SendMessageAsync<T>(T entity) where T : class;
    }
}