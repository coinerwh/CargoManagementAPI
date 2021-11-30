using System;
using System.Text;
using System.Threading.Tasks;
using CargoManagementAPI.HttpClient;
using CargoManagementAPI.Models;
using CargoManagementAPI.Queries;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CargoManagementAPI.Controllers
{
    [Route("oauth")]
    [ApiController]
    // [EnableCors("CorsPolicy")]
    public class OAuthController : Controller
    {
        private const string ClientId = "392347561763-iihp7j423rvuc6hcv1mj6so0ec85f6oa.apps.googleusercontent.com";
        private const string ClientSecret = "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";

        private readonly QueryState query;
        private readonly AuthHttpClient http;
        
        public OAuthController(QueryState queryBoats, AuthHttpClient http)
        {
            query = queryBoats;
            this.http = http;
        }

        [HttpGet]
        public async Task AcceptAndVerifyAuthorization([FromQuery] string state, string code)
        {
            var isVerified = query.VerifyState(state);
            if (!isVerified)
                return;

            var url = $"https://{Request.Host}{Request.PathBase}/oauth";
            
            var response = await http.GetAccessToken(code, ClientId, ClientSecret, url);
            var token = response.IdToken;
            
            Response.Redirect($"/userinfo?token={token}");
        }
        
        [HttpGet("signin")]
        public void EndUserSignIn()
        {
            var state = GenerateState();
            query.StoreState(state);

            var url = "https://accounts.google.com/o/oauth2/v2/auth?" +
                      "response_type=code&" +
                      $"client_id={ClientId}&" +
                      $"redirect_uri=https://{Request.Host}{Request.PathBase}/oauth&" +
                      "scope=profile&" +
                      $"state={state}";

            var SignInUrl = new SignInUrl()
            {
                Url = url
            };

            Response.Redirect(url);
        }

        [HttpGet("user")]
        public async Task<ActionResult> GetUserInfo([FromQuery] string token)
        {
            var userInfo = await http.GetUserData(token);
            return Ok(userInfo);
        }

        private string GenerateState()
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();

            char letter;

            for (var i = 0; i < 15; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                builder.Append(letter); 
            }

            return builder.ToString();
        }
    }
}