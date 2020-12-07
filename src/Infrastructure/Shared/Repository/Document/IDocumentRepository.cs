using Infrastructure.Shared.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Shared.Repository.Document
{
    public interface IDocumentRepository<T> where T : IDocument
    {
        Task<T> CreateAsync(T entity, string partitionKey);
        Task DeleteAsync(string id, string partitionKey);
        Task<T> GetByIdAsync(string id, string partitionKey);
        Task<IEnumerable<T>> GetByQueryAsync(string query, string partitionKey);
        Task<T> ReplaceAsync(T entity, string partitionKey);
    }
}