using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class LoginsRepository: ILoginsRepository
{
    // Normal
    private readonly IMongoCollection<Logins> _loginsDailyCollection;
    private readonly IMongoCollection<Logins> _loginsWeeklyCollection;
    private readonly IMongoCollection<Logins> _loginsMonthlyCollection;
    private readonly IMongoCollection<Logins> _loginsYearlyCollection;

    // Unique
    private readonly IMongoCollection<Logins> _uniqueloginsDailyCollection;
    private readonly IMongoCollection<Logins> _uniqueloginsWeeklyCollection;
    private readonly IMongoCollection<Logins> _uniqueloginsMonthlyCollection;
    private readonly IMongoCollection<Logins> _uniqueloginsYearlyCollection;

    // Normal =============================================

    public LoginsRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
	{
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _loginsDailyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.LoginsDailyCollectionName);
        _loginsWeeklyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.LoginsWeeklyCollectionName);
        _loginsMonthlyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.LoginsMonthlyCollectionName);
        _loginsYearlyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.LoginsYearlyCollectionName);

        _uniqueloginsDailyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsDailyCollectionName);
        _uniqueloginsWeeklyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsWeeklyCollectionName);
        _uniqueloginsMonthlyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsMonthlyCollectionName);
        _uniqueloginsYearlyCollection = mongoDatabase.GetCollection<Logins>(
            urbanoStoreDatabaseSettings.Value.UniqueLoginsYearlyCollectionName);
    }


    public async Task<Logins?> IncrementLoginsDailyValueAsync(DateTime date, int incrementBy)
    {
        var dayString = date.ToString("yyyy-MM-dd");
        var filter = Builders<Logins>.Filter.Eq(r => r.DailyLoginDate, dayString);

        // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
        var update = Builders<Logins>.Update
            .SetOnInsert(r => r.DailyLoginDate, dayString)
            .Inc(r => r.DailyLoginCount, incrementBy);

        var options = new FindOneAndUpdateOptions<Logins>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var updatedLogin = await _loginsDailyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedLogin;
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }


    public async Task<Logins?> IncrementLoginsWeeklyValueAsync(DateTime date, int incrementBy)
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

        // We filter by that Monday date to ensure all logins in the same week 
        // go into the same document
        var filter = Builders<Logins>.Filter.Eq(x => x.WeeklyLoginDate, weekString);

        
        var update = Builders<Logins>.Update
            .SetOnInsert(r => r.WeeklyLoginDate, weekString)
            .Inc(r => r.WeeklyLoginCount, incrementBy);

        var options = new FindOneAndUpdateOptions<Logins>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var updatedLogin = await _loginsWeeklyCollection
                .FindOneAndUpdateAsync(filter, update, options);

            return updatedLogin;
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }

    public async Task<Logins?> IncrementLoginsMonthlyValueAsync(DateTime date, int incrementBy)
    {
        var monthString = date.ToString("yyyy-MM");

        var filter = Builders<Logins>.Filter.Eq(r => r.MonthlyLoginDate, monthString);

        // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
        var update = Builders<Logins>.Update
            .SetOnInsert(r => r.MonthlyLoginDate, monthString)
            .Inc(r => r.MonthlyLoginCount, incrementBy);

        var options = new FindOneAndUpdateOptions<Logins>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var updatedLogin = await _loginsMonthlyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedLogin;
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }

    public async Task<Logins?> IncrementLoginsYearlyValueAsync(DateTime date, int incrementBy)
    {
        var yearString = date.ToString("yyyy");
        var filter = Builders<Logins>.Filter.Eq(r => r.YearlyLoginDate, yearString);

        // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
        var update = Builders<Logins>.Update
            .SetOnInsert(r => r.YearlyLoginDate, yearString)
            .Inc(r => r.YearlyLoginCount, incrementBy);

        var options = new FindOneAndUpdateOptions<Logins>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var updatedLogin = await _loginsYearlyCollection
                .FindOneAndUpdateAsync(filter, update, options);
            return updatedLogin;
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }

    // Unique =============================================
    public async Task<Logins?> IncrementUniqueLoginsDailyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
    {
        //Only Call Function if Date != previousLoginDate
        if (date.Date != previousLoginDate.Date) {

            var dayString = date.ToString("yyyy-MM-dd");
            var filter = Builders<Logins>.Filter.Eq(r => r.DailyLoginDate, dayString);

            // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
            var update = Builders<Logins>.Update
                .SetOnInsert(r => r.DailyLoginDate, dayString)
                .Inc(r => r.DailyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<Logins>
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

    public async Task<Logins?> IncrementUniqueLoginsWeeklyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
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
            var filter = Builders<Logins>.Filter.Eq(x => x.WeeklyLoginDate, weekString);

            
            var update = Builders<Logins>.Update
                .SetOnInsert(r => r.WeeklyLoginDate, weekString)
                .Inc(r => r.WeeklyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<Logins>
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

    public async Task<Logins?> IncrementUniqueLoginsMonthlyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
    {
        if (date.Year != previousLoginDate.Year || date.Month != previousLoginDate.Month) {
            var monthString = date.ToString("yyyy-MM");

            var filter = Builders<Logins>.Filter.Eq(r => r.MonthlyLoginDate, monthString);

            // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
            var update = Builders<Logins>.Update
                .SetOnInsert(r => r.MonthlyLoginDate, monthString)
                .Inc(r => r.MonthlyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<Logins>
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

    public async Task<Logins?> IncrementUniqueLoginsYearlyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy)
    {
        if (date.Year != previousLoginDate.Year) {
            var yearString = date.ToString("yyyy");
            var filter = Builders<Logins>.Filter.Eq(r => r.YearlyLoginDate, yearString);

            // Remove SetOnInsert for DailyLoginCount to avoid conflict with Inc
            var update = Builders<Logins>.Update
                .SetOnInsert(r => r.YearlyLoginDate, yearString)
                .Inc(r => r.YearlyLoginCount, incrementBy);

            var options = new FindOneAndUpdateOptions<Logins>
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