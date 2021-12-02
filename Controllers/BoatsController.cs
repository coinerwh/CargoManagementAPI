using System.Threading.Tasks;
using CargoManagementAPI.Models;
using CargoManagementAPI.Queries;
using CargoManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CargoManagementAPI.Controllers
{
    [Route("boats")]
    [ApiController]
    public class BoatsController : Controller
    {
        private ValidationService valService;
        private IQueryBoats query;
        
        public BoatsController(IQueryBoats queryBoats, ValidationService service)
        {
            query = queryBoats;
            valService = service;
        }
        
        [HttpGet]
        public async Task<ActionResult> GetBoats([FromQuery]string pageCursor = "")
        {
            var uriString = 
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";
            var baseUri = $"https://{this.Request.Host}";
            
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
            var tokenSubject = await valService.ValidateAndGetAuthTokenSubject(HttpContext);
            var boatsResult = query.GetBoatsQuery(uriString, baseUri, tokenSubject, pageCursor);
            return Ok(boatsResult);
        }

        [HttpGet("{boatId}")]
        public async Task<ActionResult> GetBoat(long boatId)
        {
            var tokenSubject = await valService.ValidateAndGetAuthTokenSubject(HttpContext);
            if (tokenSubject == null)
            {
                var error = new ErrorMessage("Token not provided or is invalid");
                return StatusCode(403,error);
            }
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
            var boat = query.GetBoatQuery(boatId, uriString, baseUri, tokenSubject);

            if (boat == null)
            {
                var error = new ErrorMessage("No boat with this boat_id exists or token provided is not owner of boat");
                return NotFound(error);   
            }

            return Ok(boat);
        }
        
        [HttpPatch("{boatId}")]
        public async Task<ActionResult> UpdateBoat([FromBody] BoatDto editedBoat, long boatId)
        {
            var tokenSubject = await valService.ValidateAndGetAuthTokenSubject(HttpContext);
            if (tokenSubject == null)
            {
                var error = new ErrorMessage("Token not provided or is invalid");
                return StatusCode(403,error);
            }
            
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

            var boat = query.UpdateBoatQuery(boatId, editedBoat, uriString, tokenSubject);

            if (boat == null)
            {
                var error = new ErrorMessage("No boat with this boat_id exists or token provided is not owner of boat");
                return NotFound(error);
            }

            return Ok(boat);
        }
        
        [HttpPut("{boatId}")]
        public async Task<ActionResult> EditBoat([FromBody] BoatDto editedBoat, long boatId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
            var tokenSubject = await valService.ValidateAndGetAuthTokenSubject(HttpContext);
            if (tokenSubject == null)
            {
                var error = new ErrorMessage("Token not provided or is invalid");
                return StatusCode(403,error);
            }

            var verifyError = VerifyInput(editedBoat, tokenSubject);
            if (verifyError != null)
                return BadRequest(verifyError);

            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";

            var boat = query.EditBoatQuery(boatId, editedBoat, uriString, tokenSubject);

            if (boat == null)
            {
                var error = new ErrorMessage("No boat with this boat_id exists or token provided is not owner of boat");
                return NotFound(error);
            }

            return this.SeeOther(boat.Self);
        }

        [HttpPost]
        public async Task<ActionResult> CreateBoat([FromBody] BoatDto newBoat)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
            if (newBoat.Name == null || newBoat.Type == null || newBoat.Length == null)
            {
                var error = new ErrorMessage("The request object is missing at least one of the required attributes");
                return BadRequest(error);
            }
            
            var tokenSubject = await valService.ValidateAndGetAuthTokenSubject(HttpContext);
            if (tokenSubject == null)
            {
                var error = new ErrorMessage("Token not provided or is invalid");
                return StatusCode(401,error);
            }

            var uriString =
                $"{$"https://{this.Request.Host}{this.Request.PathBase}{this.Request.Path}"}";

            var boatResult = query.CreateBoatQuery(newBoat, uriString, tokenSubject);

            return StatusCode(201, boatResult);
        }

        [HttpPut("{boatId}/loads/{loadId}")]
        public ActionResult AddLoadToBoat(long boatId, long loadId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
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
        public async Task<ActionResult> DeleteBoat(long boatId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
            var tokenSubject = await valService.ValidateAndGetAuthTokenSubject(HttpContext);
            if (tokenSubject == null)
            {
                var error = new ErrorMessage("Token not provided or is invalid");
                return StatusCode(403,error);
            }
            
            var deleteSuccess = query.DeleteBoatQuery(boatId, tokenSubject);

            if (!deleteSuccess)
            {
                var error = new ErrorMessage("No boat with this boat_id exists or token provided doesn't match boat owner");
                return NotFound(error);
            }

            return NoContent();
        }

        [HttpDelete("{boatId}/loads/{loadId}")]
        public ActionResult RemoveLoadFromBoat(long boatId, long loadId)
        {
            // check that Accept response type is JSON or any
            var acceptType = Request.Headers["Accept"];
            if (!acceptType.Contains("application/json") && !acceptType.Contains("*/*"))
            {
                var error = new ErrorMessage("API does not return requests of type " + acceptType +
                                             ". API can only return application/json");
                return StatusCode(406, error);
            }
            
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

        private ErrorMessage VerifyInput(BoatDto newBoat, string tokenSubject)
        {
            //verify required attributes
            if (newBoat.Name == null || newBoat.Type == null || newBoat.Length == null)
            {
                var error = new ErrorMessage("The request object is missing at least one of the required attributes");
                return error;
            }

            return null;
        }
    }
}