using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface ISimulationsRepository
{
    public Task<Simulations?> IncrementSimulationsDailyValueAsync(DateTime date, int incrementBy);
    public Task<Simulations?> IncrementSimulationsWeeklyValueAsync(DateTime date, int incrementBy);
    public Task<Simulations?> IncrementSimulationsMonthlyValueAsync(DateTime date, int incrementBy);
    public Task<Simulations?> IncrementSimulationsYearlyValueAsync(DateTime date, int incrementBy);
}

