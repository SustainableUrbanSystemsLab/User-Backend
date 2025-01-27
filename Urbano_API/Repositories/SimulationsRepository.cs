using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class SimulationsRepository: ISimulationsRepository
{
    private readonly IMongoCollection<Simulations> _simulationsDailyCollection;
    private readonly IMongoCollection<Simulations> _simulationsWeeklyCollection;
    private readonly IMongoCollection<Simulations> _simulationsMonthlyCollection;
    private readonly IMongoCollection<Simulations> _simulationsYearlyCollection;

    public SimulationsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _simulationsDailyCollection = mongoDatabase.GetCollection<Simulations>(
            urbanoStoreDatabaseSettings.Value.SimulationsDailyCollectionName);
        _simulationsWeeklyCollection = mongoDatabase.GetCollection<Simulations>(
            urbanoStoreDatabaseSettings.Value.SimulationsWeeklyCollectionName);
        _simulationsMonthlyCollection = mongoDatabase.GetCollection<Simulations>(
            urbanoStoreDatabaseSettings.Value.SimulationsMonthlyCollectionName);
        _simulationsYearlyCollection = mongoDatabase.GetCollection<Simulations>(
            urbanoStoreDatabaseSettings.Value.SimulationsYearlyCollectionName);
    }

    public async Task<Simulations?> IncrementSimulationsDailyValueAsync(DateTime date, int incrementBy)
    {
        var dateOnly = date.ToString("yyyy-MM-dd");
        var filter = Builders<Simulations>.Filter.Eq(r => r.Date, dateOnly);
        var update = Builders<Simulations>.Update
            .SetOnInsert(r => r.Date, dateOnly)
            .Inc(r => r.SimulationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Simulations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedSimulation = await _simulationsDailyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedSimulation;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing daily Simulations.", ex);
        }
    }

    public async Task<Simulations?> IncrementSimulationsWeeklyValueAsync(DateTime date, int incrementBy)
    {
        
        DateTime GetMonday(DateTime inputDate)
        {
            int dayOfWeek = (int)inputDate.DayOfWeek;  
            // 'dayOfWeek' => Sunday=0, Monday=1, ... Saturday=6

            // If date is Sunday (0), we want to go back 6 days; if Monday (1), go back 0 days; etc.
            // This formula effectively sets Monday=0, Tuesday=1, ... Sunday=6
            // so subtracting that from the date gets us back to Monday.
            int offset = (dayOfWeek == 0) ? 6 : dayOfWeek - 1; 
            return inputDate.Date.AddDays(-offset);
        }
        // Compute Monday of the given date's week
        var startOfWeek = GetMonday(date);  // uses the helper from above

        var weekString = startOfWeek.ToString("yyyy-MM-dd");
        var filter = Builders<Simulations>.Filter.Eq(r => r.Date, weekString);
        var update = Builders<Simulations>.Update
            .SetOnInsert(r => r.Date, weekString)
            .Inc(r => r.SimulationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Simulations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedSimulation = await _simulationsWeeklyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedSimulation;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing weekly Simulations.", ex);
        }
    }

    public async Task<Simulations?> IncrementSimulationsMonthlyValueAsync(DateTime date, int incrementBy)
    {
        var monthString = date.ToString("yyyy-MM");
        var filter = Builders<Simulations>.Filter.Eq(r => r.Date, monthString);
        var update = Builders<Simulations>.Update
            .SetOnInsert(r => r.Date, monthString)
            .Inc(r => r.SimulationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Simulations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedSimulation = await _simulationsMonthlyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedSimulation;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing monthly Simulations.", ex);
        }
    }

    public async Task<Simulations?> IncrementSimulationsYearlyValueAsync(DateTime date, int incrementBy)
    {
        var yearString = date.ToString("yyyy");
        var filter = Builders<Simulations>.Filter.Eq(r => r.Date, yearString);
        var update = Builders<Simulations>.Update
            .SetOnInsert(r => r.Date, yearString)
            .Inc(r => r.SimulationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Simulations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedSimulation = await _simulationsYearlyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedSimulation;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing yearly Simulations.", ex);
        }
    }
}