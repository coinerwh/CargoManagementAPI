using System;
using System.Collections.Generic;
using CargoManagementAPI.Models;
using Google.Cloud.Datastore.V1;

namespace CargoManagementAPI.Queries
{
    public class QueryUsers : IQueryUsers
    {
        private string projectId = "cargomanagementapi";
        private DatastoreDb db;
        private KeyFactory keyFactory;
        
        public QueryUsers()
        {
            db = DatastoreDb.Create(projectId);
            keyFactory = db.CreateKeyFactory("Users");
        }
        public List<string> GetUserIdsQuery()
        {
            var userSubList = new List<string>();
            Query query = new Query("Users");
            var results = db.RunQuery(query);
            var users = results.Entities;

            foreach (var user in users)
            {
                var currSub = (string) user["userId"];
                userSubList.Add(currSub);
            }

            return userSubList;
        }
        
        public List<User> GetUsersQuery()
        {
            var userList = new List<User>();
            Query query = new Query("Users");
            var results = db.RunQuery(query);
            var users = results.Entities;

            foreach (var user in users)
            {
                var currSub = (string) user["userId"];
                var newUser = new User()
                {
                    UserId = currSub
                };
                userList.Add(newUser);
            }

            return userList;
        }

        public void CreateUserQuery(string userId)
        {
            Entity newUser = new Entity()
            {
                Key = keyFactory.CreateIncompleteKey(),
                ["userId"] = userId
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Insert(newUser);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                Console.WriteLine($"Inserted key: {insertedKey}");
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {newUser.Key}");
            }
        }
    }
}