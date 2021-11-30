using Newtonsoft.Json;

namespace OAuthWebApp.Models
{
    public class UserInfo
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }
        [JsonProperty("familyName")]
        public string FamilyName { get; set; }
    }
}