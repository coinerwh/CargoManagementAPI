using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CargoManagementAPI.Models;
using Newtonsoft.Json;
using OAuthWebApp.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace CargoManagementAPI.HttpClient
{
    public class AuthHttpClient : System.Net.Http.HttpClient
    {
        public async Task<AccessTokenDto> GetAccessToken(string code, string clientId, string clientSecret, string url)
        {
            var codeDto = new VerifyCode()
            {
                Code = code,
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = url
            };

            var contentString = JsonSerializer.Serialize(codeDto);
            var content = new StringContent(contentString, Encoding.UTF8, "application/json");

            var response = await PostAsync("https://oauth2.googleapis.com/token", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<AccessTokenDto>(responseString);
            return token;
        }

        public async Task<UserInfo> GetUserData(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://people.googleapis.com/v1/people/me?personFields=names");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            var response = await SendAsync(request);
            var resString = await response.Content.ReadAsStringAsync();
            var userDto = JsonConvert.DeserializeObject<UserDto>(resString);
            var userInfo = userDto.Names[0];

            return userInfo;
        }
    }
}