using System.Collections.Generic;
using System.Threading.Tasks;
using Urbano_API.Models;

namespace Urbano_API.Interfaces
{
    public interface IWalletRepository
    {
        Task<List<Wallet>> GetAsync();
        Task<Wallet?> GetAsync(string id);
        Task<Wallet?> GetWalletByUserIdAsync(string userId);
        Task CreateAsync(Wallet wallet);
        Task UpdateAsync(string id, Wallet wallet);
        Task RemoveAsync(string id);

        // Add missing methods
        Task AddTokenAsync(string userId, string tokenType, int quantity);
        Task<bool> RemoveTokenAsync(string userId, string tokenType, int quantity);
        Task<Dictionary<string, int>> GetBalanceAsync(string userId);
        Task<bool> VerifyTokenAsync(string userId, string tokenType, int requiredQuantity);
    }
}

