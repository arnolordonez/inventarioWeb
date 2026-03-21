using MongoDB.Driver;
using InventarioWEB.Mongo;

namespace InventarioWEB.Mongo.Context
    {
        public class MongoDbContext
        {
            private readonly IMongoDatabase _database;

            public MongoDbContext(MongoSettings settings)
            {
                var client = new MongoClient(settings.ConnectionString);
                _database = client.GetDatabase(settings.DatabaseName);
            }

            public IMongoCollection<T> GetCollection<T>(string name)
            {
                return _database.GetCollection<T>(name);
            }
        }
    }
