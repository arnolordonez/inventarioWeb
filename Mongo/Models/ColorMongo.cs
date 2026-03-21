using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class ColorMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // 🔥 nullable

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public bool Activo { get; set; }
    }
}