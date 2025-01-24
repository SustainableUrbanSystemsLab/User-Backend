using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Registrations
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime Date { get; set; } = DateTime.Today;

    public int Registrations { get; set; } = 0;

    public Registrations()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}