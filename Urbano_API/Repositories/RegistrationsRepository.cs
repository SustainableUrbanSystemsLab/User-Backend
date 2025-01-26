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
        
        return null;
    }

    public async Task<Registrations?> IncrementRegistrationsMonthlyValueAsync(DateTime date, int incrementBy)
    {
        return null;
    }

    public async Task<Registrations?> IncrementRegistrationsYearlyValueAsync(DateTime date, int incrementBy)
    {
        return null;
    }
}