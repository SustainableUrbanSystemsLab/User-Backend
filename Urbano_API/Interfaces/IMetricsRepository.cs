using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface IMetricsRepository
{
    public Task<Metrics?> GetMetricsByNameAsync(string name);
    public Task<Metrics> CreateMetricsAsync(Metrics metrics);
    public Task<Metrics?> IncrementMetricsValueAsync(string name, int incrementBy);
}

