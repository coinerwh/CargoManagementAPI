using System;
using System.Collections.Generic;
using System.Web;
using CargoManagementAPI.Models;
using Google.Cloud.Datastore.V1;
using Google.Protobuf;

namespace CargoManagementAPI.Queries
{
    public class QueryLoads : IQueryLoads
    {
        private string projectId = "cargomanagementapi";
        private DatastoreDb db;
        private KeyFactory keyFactory;
        
        public QueryLoads()
        {
            db = DatastoreDb.Create(projectId);
            keyFactory = db.CreateKeyFactory("Load");
        }
        
        public LoadsDto GetLoadsQuery(string uriString, string baseUri, string pageCursor = "")
        {
            pageCursor = HttpUtility.HtmlDecode(pageCursor);
            var loadArray = new List<LoadDto>();

            Query query = new Query("Load")
            {
                Limit = 3,
            };
            if (!string.IsNullOrEmpty(pageCursor))
                query.StartCursor = ByteString.FromBase64(pageCursor);

            var results = db.RunQuery(query);
            var moreResults = results.MoreResults;
            
            var newPageCursor = moreResults == QueryResultBatch.Types.MoreResultsType.NoMoreResults ? null : 
                HttpUtility.UrlEncode(results.EndCursor?.ToBase64());
            
            var loads = results.Entities;

            foreach (Entity currLoad in loads)
            {
                var load = new LoadDto()
                {
                    Id = currLoad.Key.Path[0].Id,
                    Volume = (int) currLoad["volume"],
                    Carrier = CreateCarrier((long?) currLoad["carrier"], baseUri),
                    Content = (string) currLoad["content"],
                    CreationDate = (string) currLoad["creation_date"],
                    Self = uriString + $"/{currLoad.Key.Path[0].Id}"
                };
                loadArray.Add(load);
            }

            if (uriString != "")
            {
                if (uriString[^1] == '/')
                    uriString = uriString.Remove(uriString.Length - 1, 1);
            }
            
            LoadsDto loadsResult = new LoadsDto()
            {
                Results = loadArray,
                Next = newPageCursor == null ? null : uriString + $"/?pageCursor={newPageCursor}"
            };

            return loadsResult;
        }

        public LoadDto GetLoadQuery(long loadId, string uriString, string baseUri)
        {
            Query query = new Query("Load")
            {
                Filter = Filter.Equal("__key__", 
                    keyFactory.CreateKey(loadId))
            };
            var results = db.RunQuery(query).Entities;

            if (results.Count == 0)
            {
                return null;
            }

            var result = results[0];
            var load = new LoadDto()
            {
                Id = result.Key.Path[0].Id,
                Volume = (int) result["volume"],
                Carrier = CreateCarrier((long?) result["carrier"], baseUri),
                Content = (string) result["content"],
                CreationDate = (string) result["creation_date"],
                Self = uriString
            };

            return load;
        }

        public LoadDto CreateLoadQuery(LoadDto newLoad, string uriString)
        {
            long key;

            Entity load = new Entity()
            {
                Key = keyFactory.CreateIncompleteKey(),
                ["volume"] = newLoad.Volume,
                ["content"] = newLoad.Content,
                ["carrier"] = Value.ForNull(),
                ["creation_date"] = DateTime.UtcNow.ToShortDateString()
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Insert(load);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                Console.WriteLine($"Inserted key: {insertedKey}");
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {load.Key}");
                if (insertedKey == null)
                    return null;
                
                key = commitResponse.MutationResults[0].Key.Path[0].Id;
            }

            var loadResult = new LoadDto()
            {
                Id = key,
                Volume = newLoad.Volume,
                Carrier = null,
                Content = newLoad.Content,
                CreationDate = DateTime.UtcNow.ToShortDateString(),
                Self = uriString + $"/{key}"
            };

            return loadResult;
        }

        public bool DeleteLoadQuery(long loadId)
        {
            Query query = new Query("Load")
            {
                Filter = Filter.Equal("__key__", 
                    keyFactory.CreateKey(loadId))
            };
            var results = db.RunQuery(query).Entities;

            if (results.Count == 0)
            {
                return false;
            }

            // remove loadid from boat loads array
            var boat = GetLoadQuery(loadId, "", "").Carrier;
            if (boat != null)
            {
                var boatQuery = new QueryBoats();
                boatQuery.RemoveLoadFromBoatQuery(boat.Id, loadId);
            }
            
            // remove load
            Entity load = new Entity()
            {
                Key = keyFactory.CreateKey(loadId)
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Delete(load);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {load.Key}");
            }
            
            var result = this.GetLoadQuery(loadId, "", "");
            return result == null;
        }

        public void AddBoatToLoad(long boatId, long loadId, LoadDto loadResult)
        {
            Entity load = new Entity()
            {
                Key = keyFactory.CreateKey(loadId),
                ["carrier"] = boatId,
                ["content"] = loadResult.Content,
                ["creation_date"] = loadResult.CreationDate,
                ["volume"] = loadResult.Volume
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Update(load);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {load.Key}");
            }
        }

        public void RemoveBoatFromLoad(long loadId, LoadDto loadResult)
        {
            Entity load = new Entity()
            {
                Key = keyFactory.CreateKey(loadId),
                ["carrier"] = Value.ForNull(),
                ["content"] = loadResult.Content,
                ["creation_date"] = loadResult.CreationDate,
                ["volume"] = loadResult.Volume
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Update(load);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {load.Key}");
            }
        }

        private CarrierDto CreateCarrier(long? boatId, string baseUri)
        {
            if (boatId == null)
                return null;

            var uriString = baseUri + $"/boats/{boatId}";

            var boatQuery = new QueryBoats();
            var boat = boatQuery.GetBoatQuery((long) boatId, uriString, baseUri);
            if (boat == null)
                return null;
            
            var carrier = new CarrierDto()
            {
                Id = boat.Id,
                Name = boat.Name,
                Self = boat.Self
            };
            return carrier;
        }
    }
}