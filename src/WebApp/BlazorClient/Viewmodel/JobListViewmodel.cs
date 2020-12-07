using Newtonsoft.Json;
using Infrastructure.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorClient.Viewmodel
{
    public class JobListViewmodel
    {
        public IEnumerable<JobListItem> Jobs { get; set; }
    }

    public class JobListItem 
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

        [JsonProperty(PropertyName = "finished")]
        public DateTime? Finished { get; set; }

        [JsonProperty(PropertyName = "jobStatus")]
        public JobStatus JobStatus { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }
    }
}
