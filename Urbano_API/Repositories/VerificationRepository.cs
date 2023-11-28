using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class VerificationRepository: IVerificationRepository
{
    private readonly IMongoCollection<Verification> _verificationCollection;

    public VerificationRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _verificationCollection = mongoDatabase.GetCollection<Verification>(
            urbanoStoreDatabaseSettings.Value.VerificationsCollectionName);
    }

    public async Task UpsertAsync(string otp, string userName)
    {
        var resp = await _verificationCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();
        if (resp == null)
        {
            Verification verification = new Verification();
            verification.UserName = userName;
            verification.OTP = otp;
            await _verificationCollection.InsertOneAsync(verification);
        }
        else
        {
            resp.OTP = otp;
            await _verificationCollection.ReplaceOneAsync(x => x.UserName == userName, resp);
        }
    }

    public async Task<Verification?> GetUserAsync(string userName) =>
        await _verificationCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();
}

