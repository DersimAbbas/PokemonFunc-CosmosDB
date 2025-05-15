using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureLabb3.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AzureLabb3.Services
{
    public class CosmosRepository
    {
        private IMongoDatabase _database;

        public CosmosRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }
        public async Task<int> GetNextPokemonIdAsync()
        {
            var collection = _database.GetCollection<Pokemon>("Pokemon");
            var lastPokemon = await collection.Find(Builders<Pokemon>.Filter.Empty)
                .Sort(Builders<Pokemon>.Sort.Descending("pokemonId"))
                .Limit(1)
                .FirstOrDefaultAsync();
            return (lastPokemon?.pokemonId ?? 0) + 1;
        }
        public async Task<T?> AddAsync<T>(string collectionName, T item)
        {
            try
            {
                var collection = _database.GetCollection<T>(collectionName);
                await collection.InsertOneAsync(item);
                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item to Cosmos DB: {ex.Message}");
                return default;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string collectionName)

        {
            var collection = _database.GetCollection<T>(collectionName);
            return await collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        }

        public async Task<T?> GetByIdAsync<T>(string collectionName, int id)
        {
            var collection = _database.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("pokemonId", id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T?> UpdateAsync<T>(string collectionName, int id, T item)
        {
            var collection = _database.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("pokemonId", id);
            var result = await collection.ReplaceOneAsync(
                filter,
                item,
                new ReplaceOptions { IsUpsert = false }
            );
            return result.ModifiedCount > 0 ? item : default;
        }

        public async Task<bool> DeleteAsync<T>(string collectionName, int id)
        {
            var collection = _database.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("pokemonId", id);
            var result = await collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}
