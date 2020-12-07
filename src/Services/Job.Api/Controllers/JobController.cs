using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Shared.Repository.Document;
using Infrastructure.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Job.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IDocumentRepository<Infrastructure.Shared.Model.Job> _repository;
        private readonly IQueueService _queueService;

        public JobController(IDocumentRepository<Infrastructure.Shared.Model.Job> repository, IQueueService queueService)
        {
            _repository = repository;
            _queueService = queueService;
        }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="job">Job info</param>
        [ProducesResponseType(200,Type = typeof(Infrastructure.Shared.Model.Job))]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Infrastructure.Shared.Model.Job job) 
        {
            job.Id = Guid.NewGuid().ToString();
            job.Created = DateTime.UtcNow;
            job.Finished = null;
            job.CreatedBy = GetClaimName();
            job.JobStatus = Infrastructure.Shared.JobStatus.Created;

            await _repository.CreateAsync(job,job.CreatedBy);
            await _queueService.SendMessageAsync(job);

            return Ok(job);
        }

        /// <summary>
        /// Get jobs for the logged users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Infrastructure.Shared.Model.Job>))]
        public async Task<IActionResult> Get() 
        {
            string name = GetClaimName();

            string query = $"SELECT * FROM jobs WHERE jobs.createdBy = '{name}'";

            var jobs = await _repository.GetByQueryAsync(query, name);

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id) 
        {
            var job = await _repository.GetByIdAsync(id, GetClaimName());

            return Ok(job);
        }

        private string GetClaimName() 
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var claim = claimsIdentity.FindFirst("name");

            return claim.Value;
        }
    }


}
