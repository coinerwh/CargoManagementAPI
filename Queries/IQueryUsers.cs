using System.Collections.Generic;
using CargoManagementAPI.Models;

namespace CargoManagementAPI.Queries
{
    public interface IQueryUsers
    {
        public List<string> GetUserIdsQuery();
        public void CreateUserQuery(string userId);
        public List<User> GetUsersQuery();
    }
}