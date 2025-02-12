namespace Urbano_API.DTOs;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


public class SimulationsDTO
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }
    public string TokenType { get; set; } = string.Empty;
}


