using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Metrics 
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Logins { get; set; } = 0;

    public Metrics() {
        Id = ObjectId.GenerateNewId().ToString();
    }

}
