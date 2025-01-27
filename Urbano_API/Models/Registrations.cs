using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Registrations
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Date")]
    public string Date { get; set; } = string.Empty;

    public int RegistrationsCount { get; set; } = 0;

    public Registrations()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}