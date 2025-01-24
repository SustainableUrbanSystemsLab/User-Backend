using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Urbano_API.Models;

public class Wallet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!; // Link Wallet ot User's ID

    [BsonElement("QuotaTokens")]
    public List<QuotaToken> QuotaTokens { get; set; } = new List<QuotaToken>();

    public Wallet(string userId)
    {
        Id = ObjectId.GenerateNewId().ToString();
        UserId = userId;
    }
}

public class QuotaToken
{
    [BsonElement("Type")]
    public string Type { get; set; } = null!;

    [BsonElement("Quantity")]
    public int Quantity { get; set; } = 0;
}