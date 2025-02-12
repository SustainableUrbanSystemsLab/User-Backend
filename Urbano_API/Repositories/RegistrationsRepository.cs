using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class RegistrationsRepository: IRegistrationsRepository
{
    private readonly IMongoCollection<Registrations> _registrationsDailyCollection;
    private readonly IMongoCollection<Registrations> _registrationsWeeklyCollection;
    private readonly IMongoCollection<Registrations> _registrationsMonthlyCollection;
    private readonly IMongoCollection<Registrations> _registrationsYearlyCollection;

    public RegistrationsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _registrationsDailyCollection = mongoDatabase.GetCollection<Registrations>(
            urbanoStoreDatabaseSettings.Value.RegistrationsDailyCollectionName);
        _registrationsWeeklyCollection = mongoDatabase.GetCollection<Registrations>(
            urbanoStoreDatabaseSettings.Value.RegistrationsWeeklyCollectionName);
        _registrationsMonthlyCollection = mongoDatabase.GetCollection<Registrations>(
            urbanoStoreDatabaseSettings.Value.RegistrationsMonthlyCollectionName);
        _registrationsYearlyCollection = mongoDatabase.GetCollection<Registrations>(
            urbanoStoreDatabaseSettings.Value.RegistrationsYearlyCollectionName);
    }

    public async Task<Registrations?> IncrementRegistrationsDailyValueAsync(DateTime date, int incrementBy)
    {
        var dateOnly = date.ToString("yyyy-MM-dd");
        var filter = Builders<Registrations>.Filter.Eq(r => r.Date, dateOnly);
        var update = Builders<Registrations>.Update
            .SetOnInsert(r => r.Date, dateOnly)
            .Inc(r => r.RegistrationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Registrations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedRegistration = await _registrationsDailyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedRegistration;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing daily registrations.", ex);
        }
    }

    public async Task<Registrations?> IncrementRegistrationsWeeklyValueAsync(DateTime date, int incrementBy)
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
        var filter = Builders<Registrations>.Filter.Eq(r => r.Date, weekString);
        var update = Builders<Registrations>.Update
            .SetOnInsert(r => r.Date, weekString)
            .Inc(r => r.RegistrationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Registrations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedRegistration = await _registrationsWeeklyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedRegistration;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing weekly registrations.", ex);
        }
    }

    public async Task<Registrations?> IncrementRegistrationsMonthlyValueAsync(DateTime date, int incrementBy)
    {
        var monthString = date.ToString("yyyy-MM");
        var filter = Builders<Registrations>.Filter.Eq(r => r.Date, monthString);
        var update = Builders<Registrations>.Update
            .SetOnInsert(r => r.Date, monthString)
            .Inc(r => r.RegistrationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Registrations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedRegistration = await _registrationsMonthlyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedRegistration;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing monthly registrations.", ex);
        }
    }

    public async Task<Registrations?> IncrementRegistrationsYearlyValueAsync(DateTime date, int incrementBy)
    {
        var yearString = date.ToString("yyyy");
        var filter = Builders<Registrations>.Filter.Eq(r => r.Date, yearString);
        var update = Builders<Registrations>.Update
            .SetOnInsert(r => r.Date, yearString)
            .Inc(r => r.RegistrationsCount, incrementBy);
        var options = new FindOneAndUpdateOptions<Registrations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        try
        {
            var updatedRegistration = await _registrationsYearlyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedRegistration;
        }
        catch (MongoException ex)
        {
            throw new Exception("An error occurred while incrementing yearly registrations.", ex);
        }
    }
}