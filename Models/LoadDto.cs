using System.Text.Json.Serialization;

namespace CargoManagementAPI.Models
{
    public class LoadDto
    {
        public long Id { get; set; }
        public int? Volume { get; set; }
        public CarrierDto Carrier { get; set; }
        public string Content { get; set; }
        [JsonPropertyName("creation_date")]
        public string CreationDate { get; set; }
        public string Self { get; set; }
    }
}