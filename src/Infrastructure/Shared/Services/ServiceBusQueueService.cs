using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public class ServiceBusQueueService : IQueueService
    {
        private readonly QueueClient _queueClient;

        public ServiceBusQueueService(IConfiguration configuration)
        {
            _queueClient = new QueueClient(configuration["ServiceBus:CnxString"], configuration["ServiceBus:QueueName"]);
        }

        public async Task SendMessageAsync<T>(T entity) where T : class
        {
            string serializedObject = JsonConvert.SerializeObject(entity);

            var message = new Message(Encoding.UTF8.GetBytes(serializedObject));

            await _queueClient.SendAsync(message);
        }
    }
}
