using System.Text.Json.Serialization;

namespace CargoManagementAPI.Models
{
    public class ErrorMessage
    {
        [JsonPropertyName("Error")]
        public string Error { get; set; }

        public ErrorMessage(string errorMessage)
        {
            Error = errorMessage;
        }
    }
}