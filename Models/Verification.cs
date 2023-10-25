using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Verification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Email")]
    public string UserName { get; set; } = null!;

    public string OTP { get; set; } = null!;

    public Verification()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}