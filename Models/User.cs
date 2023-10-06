using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Email")]
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public User()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }

}