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

namespace CognitiveApi
{
    public static class CognitiveApi
    {        
        private static SpeechConfig _config;
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
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("audiofiles", FileAccess.Write, Connection = "UploadVoiceTextStorage")] CloudBlobContainer container,            
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
                   
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("The body cannot be empty, pass the text you need to convert to audio file");
            }

            var speechInfo = JsonConvert.DeserializeObject<SpeechInfo>(requestBody);
            
            if (string.IsNullOrEmpty(speechInfo.TextToConvert)) 
            {
                return new BadRequestObjectResult("The SpeechInfo object is missing mandatory parameters");
            }

            string blobUri = string.Empty;
            try
            {
                MemoryStream ms = null;                
                using (var synthesizer = new SpeechSynthesizer(SpeechConfig, null))
                {
                    var result = await synthesizer.SpeakTextAsync(speechInfo.TextToConvert);

                    if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);

                        // Something went wrong
                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            log.LogError($"ErrorCode={cancellation.ErrorCode} - ErrorDetails={cancellation.ErrorDetails}");
                            return CreateErrorResponse();
                        }
                    }
                    
                    ms = new MemoryStream(result.AudioData);                    
                }

                string filename = $"{Guid.NewGuid()}.wav";
                var blob = container.GetBlockBlobReference(filename);
                await blob.UploadFromStreamAsync(ms);
                blobUri = blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                log.LogError("Cannot process text to audio file", ex);
                return CreateErrorResponse();
            }

            return new OkObjectResult(blobUri);            
        }

        private static IActionResult CreateErrorResponse()
        {
            var result = new ObjectResult("Internal Server Error");
            result.StatusCode = StatusCodes.Status500InternalServerError;
            return result;
        }
    }
}
