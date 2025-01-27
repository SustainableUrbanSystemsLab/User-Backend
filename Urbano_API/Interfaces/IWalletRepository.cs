using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface IWalletRepository
{
    public Task<List<Wallet>> GetAsync();

    public Task<Wallet?> GetAsync(string id);

    public Task<Wallet?> GetWalletByUserIdAsync(string userId);

    public Task CreateAsync(Wallet wallet);

    public Task UpdateAsync(string id, Wallet wallet);

    public Task RemoveAsync(string id);

}