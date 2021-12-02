using System.Collections.Generic;

namespace CargoManagementAPI.Models
{
    public class BoatsDto
    {
        public List<BoatDto> Results { get; set; }
        public int TotalNumOfBoats { get; set; }
        public string Next { get; set; }
    }
}