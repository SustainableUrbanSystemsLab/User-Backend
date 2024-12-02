using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class MetricsRepository: IMetricsRepository
{
    private readonly IMongoCollection<Metrics> _metricsCollection;

    public MetricsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(
            urbanoStoreDatabaseSettings.Value.UsersCollectionName);
    }
}

