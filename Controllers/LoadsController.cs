using CargoManagementAPI.Models;
using CargoManagementAPI.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CargoManagementAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoadsController : Controller
    {
        private IQueryLoads query;

        public LoadsController(IQueryLoads queryLoads)
        {
            query = queryLoads;
        }

        [HttpGet]
        public ActionResult<LoadsDto> GetLoads([FromQuery] string pageCursor = "")
        {
            var uriString = 
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            var baseUri = $"https://{this.Request.Host}";
            var loads = query.GetLoadsQuery(uriString, baseUri, pageCursor);
            return Ok(loads);
        }

        [HttpGet("{loadId}")]
        public ActionResult<BoatDto> GetLoad(long loadId)
        {
            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            var baseUri = $"https://{this.Request.Host}";
            var load = query.GetLoadQuery(loadId, uriString, baseUri);
        
            if (load == null)
            {
                var error = new ErrorMessage("No load with this load_id exists");
                return NotFound(error);   
            }
        
            return Ok(load);
        }

        [HttpPost]
        public ActionResult CreateLoad([FromBody] LoadDto newLoad)
        {
            if (newLoad.Volume == null || newLoad.Content == null)
            {
                var error = new ErrorMessage("The request object is missing required volume or required content");
                return BadRequest(error);
            }

            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";

            var loadResult = query.CreateLoadQuery(newLoad, uriString);

            return StatusCode(201, loadResult);
        }

        [HttpPatch("{loadId}")]
        public ActionResult UpdateLoad([FromBody] LoadDto editedLoad, long loadId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            
            var load = query.UpdateLoadQuery(loadId, editedLoad, uriString);
            
            if (load == null)
            {
                var error = new ErrorMessage("No load with this load_id exists");
                return NotFound(error);
            }

            return Ok(load);
        }

        [HttpDelete("{loadId}")]
        public ActionResult DeleteLoad(long loadId)
        {
            var deleteSuccess = query.DeleteLoadQuery(loadId);
        
            if (!deleteSuccess)
            {
                var error = new ErrorMessage("No load with this load_id exists");
                return NotFound(error);
            }
        
            return NoContent();
        }
    }
}