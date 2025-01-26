using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Urbano_API.Models;
public class UniqueLogins
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string DailyLoginDate { get; set; } = "";
    public string WeeklyLoginDate { get; set; } = "";
    public string MonthlyLoginDate { get; set; } = "";
    public string YearlyLoginDate { get; set; } =  "";
    public int DailyLoginCount { get; set; } = 0;
    public int WeeklyLoginCount { get; set; } = 0;
    public int MonthlyLoginCount { get; set; } = 0;
    public int YearlyLoginCount { get; set; } = 0;
    public UniqueLogins()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}