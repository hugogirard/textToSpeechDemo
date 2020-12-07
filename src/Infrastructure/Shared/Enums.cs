using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Shared
{
    public enum JobStatus 
    { 
        Created = 0,
        Processing,
        Done,
        Error
    }
}
