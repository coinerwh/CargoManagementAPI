using System.Collections.Generic;
using CargoManagementAPI.Models;

namespace CargoManagementAPI.Queries
{
    public interface IQueryBoats
    {
        public BoatsDto GetBoatsQuery(string uriString, string baseUri, string pageCursor);
        public List<BoatDto> GetBoatsNoPagingQuery(string uriString);

        public BoatDto GetBoatQuery(long boatId, string uriString, string baseUri);
        public List<LoadDto> GetAllLoadsForBoatQuery(long boatId, string baseUri);

        public BoatDto CreateBoatQuery(BoatDto newBoat, string uriString);
        
        public (bool,bool) AddLoadToBoatQuery(long boatId, long loadId);
        public (bool, bool) RemoveLoadFromBoatQuery(long boatId, long loadId);

        public bool DeleteBoatQuery(long boatId);
    }
}