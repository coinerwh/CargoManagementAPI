using CargoManagementAPI.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CargoManagementAPI.Controllers
{
    [Route("users")]
    [ApiController]
    public class UsersController : Controller
    {
        private IQueryUsers query;
        public UsersController(IQueryUsers queryUsers)
        {
            query = queryUsers;
        }

        [HttpGet]
        public ActionResult GetUsers()
        {
            var users = query.GetUsersQuery();
            return Ok(users);
        }
    }
}