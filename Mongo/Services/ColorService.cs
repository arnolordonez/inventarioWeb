using InventarioWEB.Mongo.Models;
using InventarioWEB.Mongo.Context;
using MongoDB.Driver;

namespace InventarioWEB.Mongo.Services
{
    public class ColorService
    {
        private readonly IMongoCollection<ColorMongo> _colores;

        public ColorService(MongoDbContext context)
        {
            _colores = context.GetCollection<ColorMongo>("colores");
        }

        // LISTAR
        
        public async Task<List<ColorMongo>> GetAllAsync()
        {

            return await _colores.Find(x => x.Activo == true).ToListAsync();
        }

        public async Task<List<ColorMongo>> GetInactivosAsync()
        {
            return await _colores.Find(x => x.Activo == false).ToListAsync();
        }

        // RESTAURAR
        public async Task RestoreAsync(string id)
        {
            var update = Builders<ColorMongo>.Update.Set(x => x.Activo, true);
            await _colores.UpdateOneAsync(x => x.Id == id, update);
        }

        // CREAR
        public async Task CreateAsync(ColorMongo color)
        {
            await _colores.InsertOneAsync(color);
        }

        // OBTENER POR ID
        public async Task<ColorMongo> GetByIdAsync(string id)
        {
            return await _colores.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        // ACTUALIZAR
        public async Task UpdateAsync(string id, ColorMongo color)
        {
            await _colores.ReplaceOneAsync(x => x.Id == id, color);
        }

        // ELIMINAR
        public async Task DeleteAsync(string id)
        {
            var update = Builders<ColorMongo>.Update.Set(x => x.Activo, false);
            await _colores.UpdateOneAsync(x => x.Id == id, update);
        }
    }
}