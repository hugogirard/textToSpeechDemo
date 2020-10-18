using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SharedModel = Infrastructure.Shared.Model;

namespace BlazorClient.Services.Job
{
    public class JobService : IJobService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly string _scope;
        private readonly string _baseAddress;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _handler;
        private readonly string _apiAccessKey;

        public JobService(HttpClient httpClient,
                          IConfiguration configuration,
                          ITokenAcquisition tokenAcquisition,
                          MicrosoftIdentityConsentAndConditionalAccessHandler identityHandler)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _scope = configuration["Api:JobApiScope"];
            _baseAddress = configuration["Api:JobApi"];
            _handler = identityHandler;
            _apiAccessKey = configuration["Api:ApimKey"];            
        }

        public async Task<SharedModel.Job> CreateJob(string text)
        {
            
            await PrepareAuthenticatedClient();

            var job = new SharedModel.Job { Text = text };

            var jsonRequest = JsonConvert.SerializeObject(job);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await this._httpClient.PostAsync($"{ _baseAddress}/api/job", jsoncontent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var createdJob = JsonConvert.DeserializeObject<SharedModel.Job>(content);

                return createdJob;
            }

            return null;
        }

        public async Task<IEnumerable<SharedModel.Job>> GetJobsUser() 
        {
            await PrepareAuthenticatedClient();
          
            var response = await this._httpClient.GetAsync($"{ _baseAddress}/api/job");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jobs = JsonConvert.DeserializeObject<IEnumerable<SharedModel.Job>>(content);

                return jobs;
            }

            return null;

        }

        public async Task<SharedModel.Job> GetJobDetail(string id) 
        {
            await PrepareAuthenticatedClient();

            var response = await this._httpClient.GetAsync($"{ _baseAddress}/api/job/{id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var job = JsonConvert.DeserializeObject<SharedModel.Job>(content);

                return job;
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
