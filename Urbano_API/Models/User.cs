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

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Organization { get; set; } = null!;

    public bool Verified { get; set; } = true;

    public int AttemptsLeft { get; set; } = 4;

    public int MaxAttempts { get; set; } = 4;

    public string AffiliationType { get; set; } = null!;

    public string Role { get; set; } = Roles.USER.ToString();

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime Date { get; set; } = DateTime.Today;

    public User()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}

enum Roles
{
    ADMIN,
    USER,
    VIP
}