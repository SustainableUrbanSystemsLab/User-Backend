using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class MetricsRepository: IUserRepository
{
    private readonly IMongoCollection<User> _metricsCollection;

    public MetricsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _metricsCollection = mongoDatabase.GetCollection<User>(
            urbanoStoreDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
            await _metricsCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _metricsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetUserAsync(string userName) =>
        await _metricsCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();

    public async Task CreateAsync(User user) =>
        await _metricsCollection.InsertOneAsync(user);

    public async Task UpdateAsync(string id, User user) =>
        await _metricsCollection.ReplaceOneAsync(x => x.Id == id, user);

    public async Task RemoveAsync(string id) =>
        await _metricsCollection.DeleteOneAsync(x => x.Id == id);
}

