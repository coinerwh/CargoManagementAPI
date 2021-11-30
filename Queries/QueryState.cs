using System;
using Google.Cloud.Datastore.V1;

namespace CargoManagementAPI.Queries
{
    public class QueryState
    {
        private string projectId = "cargomanagementapi";
        private DatastoreDb db;
        private KeyFactory keyFactory;
        
        public QueryState()
        {
            db = DatastoreDb.Create(projectId);
            keyFactory = db.CreateKeyFactory("State");
        }

        public void StoreState(string state)
        {
            Entity newState = new Entity()
            {
                Key = keyFactory.CreateIncompleteKey(),
                ["state"] = state
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Insert(newState);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                Console.WriteLine($"Inserted key: {insertedKey}");
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {newState.Key}");
            }
        }

        public bool VerifyState(string userState)
        {
            Query query = new Query("State");
            var results = db.RunQuery(query);
            var states = results.Entities;

            foreach (var state in states)
            {
                var currState = (string) state["state"];
                if (currState == userState)
                    return true;
            }

            return false;
        }
    }
}