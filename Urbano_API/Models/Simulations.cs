using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Simulations
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Date")]
    public string Date { get; set; } = string.Empty;

    public string SimulationType { get; set; } = string.Empty;

    public int SimulationsCount { get; set; } = 0;

    public Simulations()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}