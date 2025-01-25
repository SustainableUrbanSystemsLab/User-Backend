using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Urbano_API.Models;
public class Logins
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime Date { get; set; } = DateTime.Today;
    public int DailyLoginCount { get; set; } = 0;
    public int WeeklyLoginCount { get; set; } = 0;
    public Logins()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}