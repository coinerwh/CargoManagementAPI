using System.Collections.Generic;

namespace CargoManagementAPI.Models
{
    public class LoadsDto
    {
        public List<LoadDto> Results { get; set; }
        public string Next { get; set; }
    }
}