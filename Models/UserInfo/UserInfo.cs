using Newtonsoft.Json;

namespace CargoManagementAPI.Models.UserInfo
{
    public class UserInfo
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }
        [JsonProperty("familyName")]
        public string FamilyName { get; set; }
    }
}