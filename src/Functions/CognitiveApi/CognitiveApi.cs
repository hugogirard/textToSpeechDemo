using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Storage.Blob;
using Microsoft.CognitiveServices.Speech;
using CognitiveApi.Model;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Azure.Storage.Queue.Protocol;
using System.Runtime.CompilerServices;
using Infrastructure.Shared.Model;

namespace CognitiveApi
{
    public static class CognitiveApi
    {
        private static SpeechConfig _config;

        private static ValetKey ValetKey => new ValetKey();

        private static SpeechConfig SpeechConfig
        {
            get
            {
                if (_config != null)
                    return _config;

                string endpoint = Environment.GetEnvironmentVariable("CognitiveServiceEndpoint");
                string subscriptionKey = Environment.GetEnvironmentVariable("CognitiveServiceSubscriptionKey");

                _config = SpeechConfig.FromEndpoint(new Uri(endpoint), subscriptionKey);
                _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);
                return _config;
            }
        }

        [FunctionName("TextToAudio")]
        public static async Task Run([ServiceBusTrigger("processtextqueue", Connection = "ServiceBusCnxString")] string queueItem,
                                     [CosmosDB(databaseName: "voicesystem",
                                               collectionName: "jobs",
                                               ConnectionStringSetting = "CosmosDBConnection")]IAsyncCollector<Job> jobs,
                                     [Blob("audiofiles", FileAccess.ReadWrite, Connection = "UploadVoiceTextStorage")] CloudBlobContainer container,
                                     ILogger log)
        {

            log.LogInformation("Processing request text to speech.");

            var job = JsonConvert.DeserializeObject<Job>(queueItem);

            string blobUri;
            bool errorProcess = false;
            try
            {
                MemoryStream ms = null;
                using (var synthesizer = new SpeechSynthesizer(SpeechConfig, null))
                {
                    log.LogInformation("SpeechSynthesizer created");

                    var text = WriteXmlFile(job.Text, job.Language);

                    var result = await synthesizer.SpeakSsmlAsync(text);
                           
                    log.LogInformation("Result for text to audio");
                    log.LogInformation($"Result reason {result.Reason}");

                    if (result.Reason == ResultReason.Canceled)
                    {
                        log.LogInformation($"Get cancellation result {result.Reason}");
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);

                        // Something went wrong
                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            log.LogError($"ErrorCode={cancellation.ErrorCode} - ErrorDetails={cancellation.ErrorDetails}");

                            job.Error = $"ErrorCode={cancellation.ErrorCode} - ErrorDetails={cancellation.ErrorDetails}";
                            job.JobStatus = Infrastructure.Shared.JobStatus.Error;
                            job.Finished = DateTime.UtcNow;
                            errorProcess = true;
                        }
                    }

                    log.LogInformation("Getting memory object");

                    ms = new MemoryStream(result.AudioData);
                }

                if (!errorProcess) 
                {
                    string filename = $"{Guid.NewGuid()}.wav";

                    log.LogInformation("Getting blob reference");

                    var blob = container.GetBlockBlobReference(filename);

                    log.LogInformation("Uploading to blob reference");

                    await blob.UploadFromStreamAsync(ms);

                    log.LogInformation("Blob uploaded");

                    job.BlobName = filename;
                    job.BlobUri = filename;
                    job.Finished = DateTime.UtcNow;
                    job.JobStatus = Infrastructure.Shared.JobStatus.Done;
                }

                
            }
            catch (Exception ex)
            {
                job.Error = ex.Message;
                job.JobStatus = Infrastructure.Shared.JobStatus.Error;
                job.Finished = DateTime.UtcNow;
            }

            await jobs.AddAsync(job);
        }

        private static string WriteXmlFile(string text, string language)
        {

            string lang = language == "en" ? "en-US" : "fr-CA";
            string voice = language == "en" ? "en-US-JennyNeural" : "fr-CA-Caroline";

            string audioTxt = @$"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:mstts=""https://www.w3.org/2001/mstts"" xml:lang=""{lang}"">
                                  <voice name=""{voice}"">
                                    <mstts:express-as style=""chat"">
                                      {text}
                                    </mstts:express-as>
                                  </voice>
                                </speak>";

            return audioTxt;
        }

        private static IActionResult CreateErrorResponse()
        {
            var result = new ObjectResult("Internal Server Error");
            result.StatusCode = StatusCodes.Status500InternalServerError;
            return result;
        }
    }
}
