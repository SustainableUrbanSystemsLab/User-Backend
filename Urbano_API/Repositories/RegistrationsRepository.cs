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
        var dateOnly = date.Date;
        var filter = Builders<Registrations>.Filter.Eq(r => r.Date, dateOnly);
        var update = Builders<Registrations>.Update
            .SetOnInsert(r => r.Date, dateOnly)
            .SetOnInsert(r => r.Registrations, 0)
            .Inc(r => r.Registrations, incrementBy);
        var options = new FindOneAndUpdateOptions<Registrations>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var updatedRegistration = await _registrationsDailyCollection
            .FindOneAndUpdateAsync(filter, update, options);

        return updatedRegistration;
    }
}