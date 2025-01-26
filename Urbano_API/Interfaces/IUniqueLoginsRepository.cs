using Urbano_API.Models;
namespace Urbano_API.Interfaces;
public interface IUniqueLoginsRepository
{
    public Task<UniqueLogins?> IncrementUniqueLoginsDailyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy);
    public Task<UniqueLogins?> IncrementUniqueLoginsWeeklyValueAsync(DateTime date, DateTime previousLoginDate, int incrementBy);
    public Task<UniqueLogins?> IncrementUniqueLoginsMonthlyValueAsync(DateTime date,  DateTime previousLoginDate, int incrementBy);
    public Task<UniqueLogins?> IncrementUniqueLoginsYearlyValueAsync(DateTime date,  DateTime previousLoginDate, int incrementBy);
}