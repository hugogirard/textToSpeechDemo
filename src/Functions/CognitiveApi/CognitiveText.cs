using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Infrastructure.Shared.Model;

namespace CognitiveApi
{
    public static class CognitiveText
    {
        private static TextAnalyticsClient _textClient;

        public static TextAnalyticsClient TextClient 
        {
            get 
            {
                if (_textClient != null)
                    return _textClient;

                var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("AnalyticCredentials"));
                _textClient = new TextAnalyticsClient(new Uri(Environment.GetEnvironmentVariable("AnalyticEndpoint")),
                                                      credential);

                return _textClient;
            }
        }


        [FunctionName("DetectText")]
        public static async Task Run([ServiceBusTrigger("analyzetextqueue", Connection = "ServiceBusCnxString")]string queueItem, 
                                     ILogger log,
                                     [CosmosDB(databaseName: "voicesystem",
                                               collectionName: "jobs",
                                               ConnectionStringSetting = "CosmosDBConnection")]IAsyncCollector<Job> jobs)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {queueItem}");

            var job = JsonConvert.DeserializeObject<Job>(queueItem);

            var response = await _textClient.DetectLanguageAsync(job.Text);

            await jobs.AddAsync(job);
        }
    }
}
