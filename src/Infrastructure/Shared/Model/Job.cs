using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Model
{
    public class Job
    {
        public string Id { get; set; }

        public string CreatedBy { get; set; }

        public string Text { get; set; }

        public DateTime Created { get; set; }

        public DateTime Finished { get; set; }

        public JobStatus JobStatus { get; set; }

        public string Language { get; set; }

        public string BlobName { get; set; }

        public string Error { get; set; }
    }

}
