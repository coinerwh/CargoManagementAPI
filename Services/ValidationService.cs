using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;

namespace CargoManagementAPI.Services
{
    public class ValidationService
    {
        private const string ClientId = "392347561763-iihp7j423rvuc6hcv1mj6so0ec85f6oa.apps.googleusercontent.com";
        
        public async Task<string> ValidateAndGetAuthTokenSubject(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                return null;
            }
            var authHeader = context.Request.Headers["Authorization"][0];
            var token = authHeader;
            if (authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }
            
            if (token.Length == 0)
            {
                return null;
            }

            var tokenPayload = await ValidateToken(token);
            return tokenPayload?.Subject;
        }
        
        public async Task<JsonWebToken.Payload> ValidateToken(string idToken)
        {
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new string[] { ClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
                return payload;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}