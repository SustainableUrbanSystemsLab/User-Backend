using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface ILoginsRepository
{
    // Normal
    public Task<Logins?> IncrementLoginsDailyValueAsync(DateTime date, int incrementBy);

    public Task<Logins?> IncrementLoginsWeeklyValueAsync(DateTime date, int incrementBy);

    public Task<Logins?> IncrementLoginsMonthlyValueAsync(DateTime date, int incrementBy);

    public Task<Logins?> IncrementLoginsYearlyValueAsync(DateTime date, int incrementBy);

    // Unique
    public Task<Logins?> IncrementUniqueLoginsDailyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy);

    public Task<Logins?> IncrementUniqueLoginsWeeklyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy);

    public Task<Logins?> IncrementUniqueLoginsMonthlyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy);

    public Task<Logins?> IncrementUniqueLoginsYearlyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy);

    public Task<int> GetUserDailyLoginCountAsync(string userId);
}