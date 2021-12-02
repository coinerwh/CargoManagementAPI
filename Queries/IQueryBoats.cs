using System.Collections.Generic;
using CargoManagementAPI.Models;

namespace CargoManagementAPI.Queries
{
    public interface IQueryBoats
    {
        public BoatsDto GetBoatsQuery(string uriString, string baseUri, string tokenSubject, string pageCursor);
        public List<BoatDto> GetBoatsNoPagingQuery(string uriString);
        public BoatDto GetBoatQuery(long boatId, string uriString, string baseUri, string tokenSubject);
        public BoatDto CreateBoatQuery(BoatDto newBoat, string uriString, string tokenSubject);
        public BoatDto UpdateBoatQuery(long boatId, BoatDto editedBoat, string uriString, string tokenSubject);
        public BoatDto EditBoatQuery(long boatId, BoatDto editedBoat, string uriString, string tokenSubject);
        public (bool,bool) AddLoadToBoatQuery(long boatId, long loadId);
        public (bool, bool) RemoveLoadFromBoatQuery(long boatId, long loadId);
        public bool DeleteBoatQuery(long boatId, string tokenSubject);
        public bool VerifyNameIsUnique(string name, string tokenSubject);
        public bool VerifyName(string name);
        public bool VerifyType(string type);
        public bool VerifyLength(int? length);
    }
}