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
            var baseUri = $"https://{this.Request.Host}";
            var loads = query.GetLoadsQuery(uriString, baseUri, pageCursor);
            return Ok(loads);
        }

        [HttpGet("{loadId}")]
        public ActionResult<BoatDto> GetLoad(long loadId)
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
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
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

        [HttpPut("{loadId}")]
        public ActionResult EditLoad([FromBody] LoadDto editedLoad, long loadId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }

            if (editedLoad.Content == null || editedLoad.Volume == null)
            {
                var error = new ErrorMessage("The request object is missing at least one of the required attributes");
                return BadRequest(error);
            }

            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";

            var load = query.EditLoadQuery(loadId, editedLoad, uriString);

            if (load == null)
            {
                var error = new ErrorMessage("No boat with this boat_id exists or token provided is not owner of boat");
                return NotFound(error);
            }

            return this.SeeOther(load.Self);
        }

        [HttpDelete("{loadId}")]
        public ActionResult DeleteLoad(long loadId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
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