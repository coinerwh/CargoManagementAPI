using System;
using System.Collections.Generic;
using System.Web;
using CargoManagementAPI.Models;
using Google.Cloud.Datastore.V1;
using Google.Protobuf;

namespace CargoManagementAPI.Queries
{
    public class QueryBoats : IQueryBoats
    {
        private string projectId = "cargomanagementapi";
        private DatastoreDb db;
        private KeyFactory keyFactory;
        
        public QueryBoats()
        {
            db = DatastoreDb.Create(projectId);
            keyFactory = db.CreateKeyFactory("Boat");
        }
        
        public BoatsDto GetBoatsQuery(string uriString, string baseUri, string pageCursor = "")
        {
            pageCursor = HttpUtility.HtmlDecode(pageCursor);
            var boatArray = new List<BoatDto>();

            Query query = new Query("Boat")
            {
                Limit = 3,
            };
            if (!string.IsNullOrEmpty(pageCursor))
                query.StartCursor = ByteString.FromBase64(pageCursor);

            var results = db.RunQuery(query);
            var moreResults = results.MoreResults;
            
            var newPageCursor = moreResults == QueryResultBatch.Types.MoreResultsType.NoMoreResults ? null : 
                HttpUtility.UrlEncode(results.EndCursor?.ToBase64());
            
            var boats = results.Entities;

            foreach (Entity currBoat in boats)
            {
                var boat = new BoatDto()
                {
                    Id = currBoat.Key.Path[0].Id,
                    Length = (int) currBoat["length"],
                    Name = (string) currBoat["name"],
                    Type = (string) currBoat["type"],
                    Loads = CreateLoadsObject((long[]) currBoat["loads"], baseUri),
                    Self = uriString + $"/{currBoat.Key.Path[0].Id}"
                };
                boatArray.Add(boat);
            }
            
            BoatsDto boatsResult = new BoatsDto()
            {
                Results = boatArray,
                Next = newPageCursor == null ? null : uriString + $"/?pageCursor={newPageCursor}"
            };

            return boatsResult;
        }
        
        public List<BoatDto> GetBoatsNoPagingQuery(string uriString)
        {
            var boatArray = new List<BoatDto>();

            Query query = new Query("Boat");
            

            var results = db.RunQuery(query);

            var boats = results.Entities;

            foreach (Entity currBoat in boats)
            {
                var boat = new BoatDto()
                {
                    Id = currBoat.Key.Path[0].Id,
                    Length = (int) currBoat["length"],
                    Name = (string) currBoat["name"],
                    Type = (string) currBoat["type"],
                    Loads = CreateLoadsObject((long[]) currBoat["loads"], uriString),
                    Self = uriString + $"/{currBoat.Key.Path[0].Id}"
                };
                boatArray.Add(boat);
            }

            return boatArray;
        }

        public BoatDto GetBoatQuery(long boatId, string uriString, string baseUri)
        {
            Query query = new Query("Boat")
            {
                Filter = Filter.Equal("__key__", 
                    keyFactory.CreateKey(boatId))
            };
            var results = db.RunQuery(query).Entities;

            if (results.Count == 0)
            {
                return null;
            }

            var result = results[0];
            var boat = new BoatDto()
            {
                Id = result.Key.Path[0].Id,
                Length = (int) result["length"],
                Name = (string) result["name"],
                Loads = CreateLoadsObject((long[]) result["loads"], baseUri),
                Type = (string) result["type"],
                Self = uriString
            };

            return boat;
        }

        public List<LoadDto> GetAllLoadsForBoatQuery(long boatId, string baseUri)
        {
            var loadQuery = new QueryLoads();
            var boat = GetBoatQuery(boatId, "", "");
            if (boat == null)
                return null;

            var loads = new List<LoadDto>();
            foreach (var boatLoad in boat.Loads)
            {
                var load = loadQuery.GetLoadQuery(boatLoad.Id, baseUri + $"/loads/{boatLoad.Id}", baseUri);
                loads.Add(load);
            }

            return loads;
        }

        public BoatDto CreateBoatQuery(BoatDto newBoat, string uriString)
        {
            long key;

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateIncompleteKey(),
                ["name"] = newBoat.Name,
                ["type"] = newBoat.Type,
                ["length"] = newBoat.Length,
                ["loads"] = Array.Empty<long>()
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Insert(boat);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                Console.WriteLine($"Inserted key: {insertedKey}");
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {boat.Key}");
                if (insertedKey == null)
                    return null;
                
                key = commitResponse.MutationResults[0].Key.Path[0].Id;
            }

            var boatResult = new BoatDto()
            {
                Id = key,
                Name = newBoat.Name,
                Length = newBoat.Length,
                Type = newBoat.Type,
                Loads = new List<BoatLoadsDto>(),
                Self = uriString + $"/{key}"
            };

            return boatResult;
        }

        public (bool,bool) AddLoadToBoatQuery(long boatId, long loadId)
        {
            var loadQuery = new QueryLoads();
            
            // check for boat and load exists
            var boatResult = GetBoatQuery(boatId, "","");
            if (boatResult == null)
                return (false,true);

            var loadResult = loadQuery.GetLoadQuery(loadId, "", "");
            if (loadResult == null)
                return (false,true);

            // check if loadId already on another boat
            var boats = GetBoatsNoPagingQuery("");
            if (CheckLoadInOtherBoat(boats, loadId))
                return (true,false);
            
            // add loadId to loads property in boat
            var newLoads = new long[boatResult.Loads.Count + 1];
            var i = 0;
            foreach (var load in boatResult.Loads)
            {
                newLoads[i++] = load.Id;
            }
            newLoads[^1] = loadId;

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateKey(boatId),
                ["loads"] = newLoads,
                ["length"] = boatResult.Length,
                ["name"] = boatResult.Name,
                ["type"] = boatResult.Type
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Update(boat);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {boat.Key}");
            }
            
            // add boatId to load
            loadQuery.AddBoatToLoad(boatId, loadId, loadResult);
            
            return (true,true);
        }

        public (bool, bool) RemoveLoadFromBoatQuery(long boatId, long loadId)
        {
            var loadQuery = new QueryLoads();
            
            // check for boat and load exists
            var boatResult = GetBoatQuery(boatId, "","");
            if (boatResult == null)
                return (false,true);

            var loadResult = loadQuery.GetLoadQuery(loadId, "", "");
            if (loadResult == null)
                return (false,true);

            // check if loadId is in boat
            var inBoat = false;
            foreach (var load in boatResult.Loads)
            {
                if (load.Id == loadId)
                    inBoat = true;
            }

            if (!inBoat)
                return (true, false);
            
            // remove loadId from loads array in Boat
            var newLoads = new long[boatResult.Loads.Count - 1];
            var i = 0;
            foreach (var load in boatResult.Loads)
            {
                if (load.Id != loadId)
                {
                    newLoads[i] = load.Id;
                }
            }

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateKey(boatId),
                ["loads"] = newLoads,
                ["length"] = boatResult.Length,
                ["name"] = boatResult.Name,
                ["type"] = boatResult.Type
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Update(boat);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {boat.Key}");
            }
            
            
            // remove boatId to load
            loadQuery.RemoveBoatFromLoad(loadId, loadResult);

            return (true, true);
        }

        private List<BoatLoadsDto> CreateLoadsObject(long[] loads, string uriString)
        {
            var loadsList = new List<BoatLoadsDto>();
            foreach (var load in loads)
            {
                var loadObject = new BoatLoadsDto()
                {
                    Id = load,
                    Self = uriString + $"/loads/{load}"
                };
                loadsList.Add(loadObject);
            }
            return loadsList;
        }

        private bool CheckLoadInOtherBoat(List<BoatDto> boats, long loadId)
        {
            foreach (var boat in boats)
            {
                foreach (var load in boat.Loads)
                {
                    if (load.Id == loadId)
                        return true;
                }
            }

            return false;
        }

        public bool DeleteBoatQuery(long boatId)
        {
            Query query = new Query("Boat")
            {
                Filter = Filter.Equal("__key__", 
                    keyFactory.CreateKey(boatId))
            };
            var results = db.RunQuery(query).Entities;

            if (results.Count == 0)
            {
                return false;
            }

            // remove boatid from load entity
            var loadQuery = new QueryLoads();
            var loads = loadQuery.GetLoadsQuery("", "").Results;
            foreach (var load in loads)
            {
                if (load.Carrier == null)
                    continue;
                if (load.Carrier.Id == boatId)
                {
                    loadQuery.RemoveBoatFromLoad(load.Id, load);
                }
            }

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateKey(boatId)
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Delete(boat);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {boat.Key}");
            }
            
            var result = this.GetBoatQuery(boatId, "", "");
            if (result == null)
                return true;

            return false;
        }
    }
}