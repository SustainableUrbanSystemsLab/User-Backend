using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class UniqueLoginsRepository: IUniqueLoginsRepository
{
    private readonly IMongoCollection<UniqueLogins> _uniqueloginsDailyCollection;
    private readonly IMongoCollection<UniqueLogins> _uniqueloginsWeeklyCollection;
    private readonly IMongoCollection<UniqueLogins> _uniqueloginsMonthlyCollection;
    private readonly IMongoCollection<UniqueLogins> _uniqueloginsYearlyCollection;

    public UniqueLoginsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _uniqueloginsDailyCollection = mongoDatabase.GetCollection<UniqueLogins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsDailyCollectionName);
        _uniqueloginsWeeklyCollection = mongoDatabase.GetCollection<UniqueLogins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsWeeklyCollectionName);
        _uniqueloginsMonthlyCollection = mongoDatabase.GetCollection<UniqueLogins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsMonthlyCollectionName);
        _uniqueloginsYearlyCollection = mongoDatabase.GetCollection<UniqueLogins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsYearlyCollectionName);
    }


    public async Task<UniqueLogins?> IncrementUniqueLoginsDailyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
    {
        //Only Call Function if Date != previousLoginDate
        if (date.Date != previousLoginDate.Date) {

            var dayString = date.ToString("yyyy-MM-dd");
            var filter = Builders<UniqueLogins>.Filter.Eq(r => r.DailyLoginDate, dayString);

            // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
            var update = Builders<UniqueLogins>.Update
                .SetOnInsert(r => r.DailyLoginDate, dayString)
                .Inc(r => r.DailyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<UniqueLogins>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            try
            {
                var updatedLogin = await _uniqueloginsDailyCollection
                    .FindOneAndUpdateAsync(filter, update, options);
                return updatedLogin;
            }
            catch (MongoCommandException ex)
            {
                Console.WriteLine($"MongoDB error: {ex.Message}");
                throw;
            }
        }
        return null;
        
    }


    public async Task<UniqueLogins?> IncrementUniqueLoginsWeeklyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
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

        var mondayPreviousWeek = GetMonday(previousLoginDate);

        bool isSameWeek = (startOfWeek == mondayPreviousWeek);

        if (!isSameWeek) {
             var weekString = startOfWeek.ToString("yyyy-MM-dd");

            // We filter by that Monday date to ensure all logins in the same week 
            // go into the same document
            var filter = Builders<UniqueLogins>.Filter.Eq(x => x.WeeklyLoginDate, weekString);

            
            var update = Builders<UniqueLogins>.Update
                .SetOnInsert(r => r.WeeklyLoginDate, weekString)
                .Inc(r => r.WeeklyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<UniqueLogins>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            try
            {
                var updatedLogin = await _uniqueloginsWeeklyCollection
                    .FindOneAndUpdateAsync(filter, update, options);

                return updatedLogin;
            }
            catch (MongoCommandException ex)
            {
                Console.WriteLine($"MongoDB error: {ex.Message}");
                throw;
            }
        }
        return null;

    }

    public async Task<UniqueLogins?> IncrementUniqueLoginsMonthlyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
    {
        if (date.Year != previousLoginDate.Year || date.Month != previousLoginDate.Month) {
            var monthString = date.ToString("yyyy-MM");

            var filter = Builders<UniqueLogins>.Filter.Eq(r => r.MonthlyLoginDate, monthString);

            // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
            var update = Builders<UniqueLogins>.Update
                .SetOnInsert(r => r.MonthlyLoginDate, monthString)
                .Inc(r => r.MonthlyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<UniqueLogins>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            try
            {
                var updatedLogin = await _uniqueloginsMonthlyCollection
                    .FindOneAndUpdateAsync(filter, update, options);
                return updatedLogin;
            }
            catch (MongoCommandException ex)
            {
                Console.WriteLine($"MongoDB error: {ex.Message}");
                throw;
            }
        }
        return null;

    }

    public async Task<UniqueLogins?> IncrementUniqueLoginsYearlyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
    {
        if (date.Year != previousLoginDate.Year) {
            var yearString = date.ToString("yyyy");
            var filter = Builders<UniqueLogins>.Filter.Eq(r => r.YearlyLoginDate, yearString);

            // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
            var update = Builders<UniqueLogins>.Update
                .SetOnInsert(r => r.YearlyLoginDate, yearString)
                .Inc(r => r.YearlyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<UniqueLogins>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            try
            {
                var updatedLogin = await _uniqueloginsYearlyCollection
                    .FindOneAndUpdateAsync(filter, update, options);
                return updatedLogin;
            }
            catch (MongoCommandException ex)
            {
                Console.WriteLine($"MongoDB error: {ex.Message}");
                throw;
            }
        }
        return null;
    }

}