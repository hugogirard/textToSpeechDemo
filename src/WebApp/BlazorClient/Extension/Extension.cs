using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobStatus = Infrastructure.Shared.JobStatus;

namespace BlazorClient.Extension
{
    public static class Extension
    {

        public static string ToString(this JobStatus value) 
        {
            switch (value)
            {
                case JobStatus.Created:
                    return "Created";
                case JobStatus.Processing:
                    return "Processing";
                case JobStatus.Done:
                    return "Done";
                case JobStatus.Error:
                    return "Error";
                default:
                    return "N/A";
            }
        }
    }
}
