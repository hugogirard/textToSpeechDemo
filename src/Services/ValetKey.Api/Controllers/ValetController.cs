using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ValetKey.Api.Service;

namespace ValetKey.Api.Controllers
{
#if !DEBUG
    [Authorize]
#endif
    [ApiController]    
    [Route("api/[controller]")]
    public class ValetController : ControllerBase
    {        
        private readonly IStorageService _storageService;

        public ValetController(IStorageService storageService)
        {            
            _storageService = storageService;
        }

        /// <summary>
        /// Return the sas of the blob storage
        /// </summary>
        /// <param name="blobName">The name of the blob</param>
        /// <returns>The SAS</returns>
        [ProducesResponseType(200,Type=typeof(string))]
        [ProducesResponseType(400)]
        [HttpGet]
        public IActionResult GetSas(string blobName) 
        {
            if (string.IsNullOrEmpty(blobName)) 
            {
                return new BadRequestObjectResult("Blob name need to be present");
            }

            var sas = _storageService.GetBlobDownloadLink(blobName);

            return Ok(sas);
        }
    }
}
