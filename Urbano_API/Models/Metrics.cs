using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Metrics 
{
    [BSonId]
    [BsonRepresentation(BsonType.ObjectId)]

    public string? Id { get; set; }

    public int counter { get; set; } = 0;

    public Metrics() {
        Id = ObjectId.GenerateNewId().toString();
    }

}
