using CargoManagementAPI.Models;
using CargoManagementAPI.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CargoManagementAPI.Controllers
{
    [Route("boats")]
    [ApiController]
    public class BoatsController : Controller
    {
        private IQueryBoats query;
        public BoatsController(IQueryBoats queryBoats)
        {
            query = queryBoats;
        }
        
        [HttpGet]
        public ActionResult<BoatsDto> GetBoats([FromQuery]string pageCursor = "")
        {
            var uriString = 
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            var baseUri = $"https://{this.Request.Host}";
            var boatsResult = query.GetBoatsQuery(uriString, baseUri, pageCursor);
            return Ok(boatsResult);
        }

        [HttpGet("{boatId}")]
        public ActionResult<BoatDto> GetBoat(long boatId)
        {
            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            var baseUri = $"https://{this.Request.Host}";
            var boat = query.GetBoatQuery(boatId, uriString, baseUri);

            if (boat == null)
            {
                var error = new ErrorMessage("No boat with this boat_id exists");
                return NotFound(error);   
            }

            return Ok(boat);
        }

        [HttpGet("{boatId}/loads")]
        public ActionResult GetAllLoadsForBoat(long boatId)
        {
            var uriString = $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            var baseUri = $"https://{this.Request.Host}";
            
            var loads = query.GetAllLoadsForBoatQuery(boatId, baseUri);

            if (loads == null)
            {
                var error = new ErrorMessage("No boat with this boat_id exists");
                return NotFound(error);  
            }

            return Ok(loads);
        }

        [HttpPost]
        public ActionResult CreateBoat([FromBody] BoatDto newBoat)
        {
            if (newBoat.Name == null || newBoat.Type == null || newBoat.Length == null)
            {
                var error = new ErrorMessage("The request object is missing at least one of the required attributes");
                return BadRequest(error);
            }

            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";

            var boatResult = query.CreateBoatQuery(newBoat, uriString);

            return StatusCode(201, boatResult);
        }

        [HttpPut("{boatId}/loads/{loadId}")]
        public ActionResult AddLoadToBoat(long boatId, long loadId)
        {
            var addResult = query.AddLoadToBoatQuery(boatId, loadId);

            if (!addResult.Item1)
            {
                var error = new ErrorMessage("No boat with this boat_id or no load with this load_id exists");
                return NotFound(error);
            }
            if (!addResult.Item2)
            {
                var error = new ErrorMessage("This load is already in another boat");
                return StatusCode(403, error);
            }
            return NoContent();
        }

        [HttpDelete("{boatId}")]
        public ActionResult DeleteBoat(long boatId)
        {
            var deleteSuccess = query.DeleteBoatQuery(boatId);

            if (!deleteSuccess)
            {
                var error = new ErrorMessage("No boat with this boat_id exists");
                return NotFound(error);
            }

            return NoContent();
        }

        [HttpDelete("{boatId}/loads/{loadId}")]
        public ActionResult RemoveLoadFromBoat(long boatId, long loadId)
        {
            var removeSuccess = query.RemoveLoadFromBoatQuery(boatId, loadId);

            if (!removeSuccess.Item1)
            {
                var error = new ErrorMessage("No boat with this boat_id or no load with this load_id exists");
                return NotFound(error);
            }
            if (!removeSuccess.Item2)
            {
                var error = new ErrorMessage("A load with this load_id is not in the boat with this boat_id");
                return StatusCode(403, error);
            }
            return NoContent();
        }
    }
}