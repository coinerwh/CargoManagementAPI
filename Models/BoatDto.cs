using System.Collections.Generic;

namespace CargoManagementAPI.Models
{
    public class BoatDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int? Length { get; set; }
        public List<BoatLoadsDto> Loads { get; set; }
        public string Self { get; set; }
    }
}