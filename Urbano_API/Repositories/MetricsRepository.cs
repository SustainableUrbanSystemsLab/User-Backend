using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Interfaces;
using Urbano_API.Models;

namespace Urbano_API.Repositories;

public class MetricsRepository : IMetricsRepository
{
    private readonly IMongoCollection<Metrics> _metricsCollection;

    public MetricsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _metricsCollection = mongoDatabase.GetCollection<Metrics>(
            urbanoStoreDatabaseSettings.Value.MetricsCollectionName);

        // Ensure SuccessfulLogins metric exists
        EnsureSuccessfulLoginsMetricExists().GetAwaiter().GetResult();
    }

    public async Task<Metrics?> GetMetricsByNameAsync(string name)
    {
        return await _metricsCollection.Find(m => m.Name == name).FirstOrDefaultAsync();
    }

    public async Task<Metrics> CreateMetricsAsync(Metrics metrics)
    {
        await _metricsCollection.InsertOneAsync(metrics);
        return metrics;
    }

    public async Task<Metrics?> IncrementMetricsValueAsync(string name, int incrementBy)
    {
        var update = Builders<Metrics>.Update.Inc(m => m.Logins, incrementBy);

        return await _metricsCollection.FindOneAndUpdateAsync<Metrics, Metrics>(
            m => m.Name == name,
            update,
            new FindOneAndUpdateOptions<Metrics, Metrics>
            {
                ReturnDocument = ReturnDocument.After
            }
        );
    }

    private async Task EnsureSuccessfulLoginsMetricExists()
    {
        var existingMetric = await _metricsCollection.Find(m => m.Name == "SuccessfulLogins").FirstOrDefaultAsync();
        if (existingMetric == null)
        {
            await _metricsCollection.InsertOneAsync(new Metrics
            {
                Name = "SuccessfulLogins",
                Logins = 0
            });
        }
    }
}