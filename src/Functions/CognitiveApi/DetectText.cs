using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CognitiveApi
{
    public static class DetectText
    {
        [FunctionName("DetectText")]
        public static void Run([ServiceBusTrigger("analyzetextqueue", Connection = "ServiceBusCnxString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
