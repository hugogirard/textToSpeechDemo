using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BlazorClient.Services.Valet
{
    public class ValetService : IValetService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly string _scope;
        private readonly string _baseAddress;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _handler;
        private readonly string _apiAccessKey;

        public ValetService(HttpClient httpClient,
                          IConfiguration configuration,
                          ITokenAcquisition tokenAcquisition,
                          MicrosoftIdentityConsentAndConditionalAccessHandler identityHandler)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _scope = configuration["Api:ValetScope"];
            _baseAddress = configuration["Api:ValetApi"];
            _handler = identityHandler;
            _apiAccessKey = configuration["Api:ApimKey"];
        }

        public async Task<string> GetSasFile(string blobname)
        {
            await PrepareAuthenticatedClient();

            var response = await this._httpClient.GetAsync($"{ _baseAddress}/api/Valet?blobName={blobname}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(content);                
            }

            return null;
        }

        private async Task PrepareAuthenticatedClient()
        {
            try
            {
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _scope });

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiAccessKey);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                _handler.HandleException(ex);
            }
        }
    }
}
