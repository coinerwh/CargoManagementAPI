using CargoManagementAPI.Models;
using CargoManagementAPI.Queries;
using Microsoft.AspNetCore.Mvc;

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