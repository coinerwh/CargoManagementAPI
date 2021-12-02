using CargoManagementAPI.Models;

namespace CargoManagementAPI.Queries
{
    public interface IQueryLoads
    {
        public LoadDto CreateLoadQuery(LoadDto newLoad, string uriString);
        public LoadsDto GetLoadsQuery(string uriString, string baseUri, string pageCursor);
        public LoadDto GetLoadQuery(long loadId, string uriString, string baseUri);
        public LoadDto UpdateLoadQuery(long loadId, LoadDto editedLoad, string uriString);
        public bool DeleteLoadQuery(long loadId);
    }
}