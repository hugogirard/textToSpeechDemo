using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Repository.Document;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Job.Api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IDocumentRepository<Shared.Model.Job> _repository;
        private readonly IQueueService _queueService;

        public JobController(IDocumentRepository<Shared.Model.Job> repository, IQueueService queueService)
        {
            _repository = repository;
            _queueService = queueService;
        }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="job">Job info</param>
        [ProducesResponseType(200)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Shared.Model.Job job) 
        {
            job.Id = Guid.NewGuid().ToString();
            job.Created = DateTime.UtcNow;

            await _repository.CreateAsync(job,job.CreatedBy);
            await _queueService.SendMessageAsync(job);

            return Ok();
        }

        [HttpGet("test")]
        public IActionResult Test() 
        {
            return new OkObjectResult("It works you have access");
        }
    }


}
