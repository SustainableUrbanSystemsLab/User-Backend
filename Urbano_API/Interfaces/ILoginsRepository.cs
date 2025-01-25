using Urbano_API.Models;
namespace Urbano_API.Interfaces;
public interface ILoginsRepository
{
    public Task<Logins?> IncrementLoginsDailyValueAsync(DateTime date, int incrementBy);
    public Task<Logins?> IncrementLoginsWeeklyValueAsync(DateTime date, int incrementBy);
    // public Task<Logins?> IncrementLoginsMonthlyValueAsync(DateTime date, int incrementBy);
    // public Task<Logins?> IncrementLoginsYearlyValueAsync(DateTime date, int incrementBy);
}