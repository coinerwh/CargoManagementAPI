using Microsoft.AspNetCore.Mvc;

namespace CargoManagementAPI.Controllers
{
    public static class ControllerExtensions
    {
        public static ActionResult SeeOther(this ControllerBase controller, string location)
        {
            controller.Response.Headers.Add("Location", location);
            return new StatusCodeResult(303);
        }
    }
}