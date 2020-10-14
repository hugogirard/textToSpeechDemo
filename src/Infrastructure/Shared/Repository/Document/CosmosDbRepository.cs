using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Shared.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repository.Document
{
    public class CosmosDbRepository<T> : IDocumentRepository<T> where T : IDocument
    {
        private readonly Container _container;

        public CosmosDbRepository(IConfiguration configuration)
        {
            var cosmosClient = new CosmosClient(configuration["CosmosDB:Endpoint"], configuration["CosmosDB:PrimaryKey"]);
            var database = cosmosClient.GetDatabase(configuration["CosmosDB:Database"]);
            _container = database.GetContainer(configuration["CosmosDB:Container"]);
        }

        public async Task<T> CreateAsync(T entity, string partitionKey)
        {
            return await _container.CreateItemAsync(entity, new PartitionKey(partitionKey));
        }

        public async Task<T> GetByIdAsync(string id, string partitionKey)
        {
            return await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
        }

        public async Task<IEnumerable<T>> GetByQueryAsync(string query, string partitionKey)
        {
            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<T> queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);

            List<T> entities = new List<T>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (T entity in currentResultSet)
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        public async Task<T> ReplaceAsync(T entity, string partitionKey)
        {
            return await _container.ReplaceItemAsync<T>(entity, entity.Id, new PartitionKey(partitionKey));
        }

        public async Task DeleteAsync(string id, string partitionKey)
        {
            await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }
    }
}
