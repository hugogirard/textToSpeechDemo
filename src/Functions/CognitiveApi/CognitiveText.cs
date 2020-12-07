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
        [return: ServiceBus("processtextqueue", Connection = "ServiceBusCnxString")]
        public static async Task<string> Run([ServiceBusTrigger("analyzetextqueue", Connection = "ServiceBusCnxString")]string queueItem, 
                                             ILogger log,
                                             [CosmosDB(databaseName: "voicesystem",
                                                       collectionName: "jobs",
                                                       ConnectionStringSetting = "CosmosDBConnection")]IAsyncCollector<Job> jobs)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {queueItem}");

            var job = JsonConvert.DeserializeObject<Job>(queueItem);

            var response = await TextClient.DetectLanguageAsync(job.Text);

            if (response.GetRawResponse().Status == 200) 
            {
                if (response.Value.Iso6391Name == "en" || response.Value.Iso6391Name == "fr")
                {
                    job.Language = response.Value.Iso6391Name;
                    job.JobStatus = Infrastructure.Shared.JobStatus.Processing;
                }
                else
                {
                    job.JobStatus = Infrastructure.Shared.JobStatus.Error;
                    job.Error = "Invalid language message, right now only english or french is supported";
                }

                await jobs.AddAsync(job);

                return JsonConvert.SerializeObject(job);
            }
            else 
            {
                job.JobStatus = Infrastructure.Shared.JobStatus.Error;
                job.Error = $"Cannot detect language, statusCode: {response.GetRawResponse().Status}";

                await jobs.AddAsync(job);

                return string.Empty;
            }

        }
    }
}
