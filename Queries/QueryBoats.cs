using System;
using System.Collections.Generic;
using System.Linq;
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

        public BoatsDto GetBoatsQuery(string uriString, string baseUri,  string tokenSubject, string pageCursor = "")
        {
            pageCursor = HttpUtility.HtmlDecode(pageCursor);
            var boatArray = new List<BoatDto>();

            Query query = new Query("Boat")
            {
                Limit = 5,
                Filter = Filter.Equal("owner", tokenSubject)
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
                    Owner = (string) currBoat["owner"],
                    Self = uriString + $"/{currBoat.Key.Path[0].Id}"
                };
                boatArray.Add(boat);
            }
            
            BoatsDto boatsResult = new BoatsDto()
            {
                Results = boatArray,
                TotalNumOfBoats = GetNumOfBoats(tokenSubject),
                Next = newPageCursor == null ? null : uriString + $"/?pageCursor={newPageCursor}"
            };

            return boatsResult;
        }

        private int GetNumOfBoats(string tokenSubject)
        {
            var boatArray = new List<BoatDto>();
            var query = new Query("Boat");
            var results = db.RunQuery(query);
            var boats = results.Entities;
            foreach(var boat in boats)
            {
                var newBoat = new BoatDto()
                {
                    Owner = (string) boat["owner"]
                };
                if (tokenSubject != null && newBoat.Owner == tokenSubject)
                    boatArray.Add(newBoat);
            }
            return boatArray.Count;
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
                    Owner = (string) currBoat["owner"],
                    Loads = CreateLoadsObject((long[]) currBoat["loads"], uriString),
                    Self = uriString + $"/{currBoat.Key.Path[0].Id}"
                };
                boatArray.Add(boat);
            }

            return boatArray;
        }

        public BoatDto GetBoatQuery(long boatId, string uriString, string baseUri, string tokenSubject)
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
                Owner = (string) result["owner"],
                Loads = CreateLoadsObject((long[]) result["loads"], baseUri),
                Type = (string) result["type"],
                Self = uriString
            };

            return boat.Owner != tokenSubject ? null : boat;
        }

        public BoatDto CreateBoatQuery(BoatDto newBoat, string uriString, string tokenSubject)
        {
            long key;

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateIncompleteKey(),
                ["name"] = newBoat.Name,
                ["type"] = newBoat.Type,
                ["length"] = newBoat.Length,
                ["owner"] = tokenSubject,
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
                Owner = tokenSubject,
                Loads = new List<BoatLoadsDto>(),
                Self = uriString + $"/{key}"
            };

            return boatResult;
        }
        
        public BoatDto UpdateBoatQuery(long boatId, BoatDto editedBoat, string uriString, string tokenSubject)
        {
            var boatResult = this.GetBoatQuery(boatId, uriString, "", tokenSubject);

            if (boatResult == null)
            {
                return null;
            }

            long[] loads = new long[boatResult.Loads.Count];
            for (var i = 0; i < boatResult.Loads.Count; i++)
            {
                loads[i] = boatResult.Loads[i].Id;
            }

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateKey(boatId),
                ["name"] = editedBoat.Name == null ? boatResult.Name : editedBoat.Name,
                ["type"] = editedBoat.Type == null ? boatResult.Type : editedBoat.Type,
                ["owner"] = tokenSubject,
                ["loads"] = loads,
                ["length"] = editedBoat.Length == null ? boatResult.Length : editedBoat.Length
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Update(boat);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {boat.Key}");
            }

            var result = this.GetBoatQuery(boatId, uriString, "", tokenSubject);

            return result;
        }
        
        public BoatDto EditBoatQuery(long boatId, BoatDto editedBoat, string uriString, string tokenSubject)
        {
            var boatResult = this.GetBoatQuery(boatId, uriString, "", tokenSubject);

            if (boatResult == null)
            {
                return null;
            }
            
            long[] loads = new long[boatResult.Loads.Count];
            for (var i = 0; i < boatResult.Loads.Count; i++)
            {
                loads[i] = boatResult.Loads[i].Id;
            }

            Entity boat = new Entity()
            {
                Key = keyFactory.CreateKey(boatId),
                ["name"] = editedBoat.Name,
                ["type"] = editedBoat.Type,
                ["owner"] = tokenSubject,
                ["loads"] = loads,
                ["length"] = editedBoat.Length
            };
            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Update(boat);
                CommitResponse commitResponse = transaction.Commit();
                Key insertedKey = commitResponse.MutationResults[0].Key;
                // The key is also propagated to the entity
                Console.WriteLine($"Entity key: {boat.Key}");
            }

            var result = this.GetBoatQuery(boatId, uriString, "", tokenSubject);

            return result;
        }

        public (bool,bool) AddLoadToBoatQuery(long boatId, long loadId)
        {
            // might need to get token subject ??
            var tokenSubject = "";
            
            var loadQuery = new QueryLoads();
            
            // check for boat and load exists
            var boatResult = GetBoatQuery(boatId, "","", tokenSubject);
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
            // might need to get token subject ???
            var tokenSubject = "";
            var loadQuery = new QueryLoads();
            
            // check for boat and load exists
            var boatResult = GetBoatQuery(boatId, "","", tokenSubject);
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
            // might need to get token subject ??
            var tokenSubject = "";
            
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
            
            var result = this.GetBoatQuery(boatId, "", "", tokenSubject);
            if (result == null)
                return true;

            return false;
        }
        
        public bool VerifyNameIsUnique(string name, string tokenSubject)
        {
            // get token subject ???
            var boats = GetBoatsQuery("", "", tokenSubject);
            foreach (var boat in boats.Results)
            {
                if (string.Equals(boat.Name, name, StringComparison.CurrentCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        public bool VerifyName(string name)
        {
            if (name.Length > 25)
                return false;

            if (!name.All(c => char.IsLetterOrDigit(c)))
                return false;

            return true;
        }

        public bool VerifyType(string type)
        {
            if (type.Length > 25)
                return false;

            if (!type.All(c => char.IsLetter(c)))
                return false;

            return true;
        }

        public bool VerifyLength(int? length)
        {
            if (length < 1)
                return false;
            return true;
        }
    }
}