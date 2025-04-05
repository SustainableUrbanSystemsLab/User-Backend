using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models
{
    public class Logins
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("DailyLoginDate")]
        public string DailyLoginDate { get; set; } = "";

        [BsonElement("WeeklyLoginDate")]
        public string WeeklyLoginDate { get; set; } = "";

        [BsonElement("MonthlyLoginDate")]
        public string MonthlyLoginDate { get; set; } = "";

        [BsonElement("YearlyLoginDate")]
        public string YearlyLoginDate { get; set; } = "";

        [BsonElement("DailyLoginCount")]
        public int DailyLoginCount { get; set; } = 0;

        [BsonElement("WeeklyLoginCount")]
        public int WeeklyLoginCount { get; set; } = 0;

        [BsonElement("MonthlyLoginCount")]
        public int MonthlyLoginCount { get; set; } = 0;

        [BsonElement("YearlyLoginCount")]
        public int YearlyLoginCount { get; set; } = 0;
        public CommunityType Community { get; set; }

        public Logins()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }
    }
}

