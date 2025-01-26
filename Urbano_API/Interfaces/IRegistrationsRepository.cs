using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface IRegistrationsRepository
{
    public Task<Registrations?> IncrementRegistrationsDailyValueAsync(DateTime date, int incrementBy);
    public Task<Registrations?> IncrementRegistrationsWeeklyValueAsync(DateTime date, int incrementBy);
    public Task<Registrations?> IncrementRegistrationsMonthlyValueAsync(DateTime date, int incrementBy);
    public Task<Registrations?> IncrementRegistrationsYearlyValueAsync(DateTime date, int incrementBy);
}

