using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Urbano_API.Interfaces;
using Urbano_API.Models;

namespace Urbano_API.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly IMongoCollection<Wallet> _wallets;

        public WalletRepository(IOptions<UrbanoStoreDatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _wallets = database.GetCollection<Wallet>(settings.Value.WalletCollectionName);
        }

        public async Task<List<Wallet>> GetAsync()
        {
            return await _wallets.Find(_ => true).ToListAsync();
        }

        public async Task<Wallet?> GetAsync(string id)
        {
            return await _wallets.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out var objectId))
            {
                throw new FormatException($"UserId '{userId}' is not a valid ObjectId.");
            }
            return await _wallets.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Wallet wallet)
        {
            await _wallets.InsertOneAsync(wallet);
        }

        public async Task UpdateAsync(string id, Wallet wallet)
        {
            await _wallets.ReplaceOneAsync(x => x.Id == id, wallet);
        }

        public async Task RemoveAsync(string id)
        {
            await _wallets.DeleteOneAsync(x => x.Id == id);
        }

        public async Task AddTokenAsync(string userId, string tokenType, int quantity)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                // Optionally, create a new wallet if not found
                wallet = new Wallet(userId);
                await CreateAsync(wallet);
            }

            var token = wallet.QuotaTokens.FirstOrDefault(t => t.Type == tokenType);
            if (token != null)
            {
                token.Quantity += quantity;
            }
            else
            {
                wallet.QuotaTokens.Add(new QuotaToken
                {
                    Type = tokenType,
                    Quantity = quantity
                });
            }

            await UpdateAsync(wallet.Id!, wallet);
        }

        public async Task<bool> RemoveTokenAsync(string userId, string tokenType, int quantity)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return false;

            var token = wallet.QuotaTokens.FirstOrDefault(t => t.Type == tokenType);
            if (token == null || token.Quantity < quantity) return false;

            token.Quantity -= quantity;

            if (token.Quantity == 0)
            {
                wallet.QuotaTokens.Remove(token);
            }

            await UpdateAsync(wallet.Id!, wallet);
            return true;
        }

        public async Task<Dictionary<string, int>> GetBalanceAsync(string userId)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return new Dictionary<string, int>();

            return wallet.QuotaTokens.ToDictionary(t => t.Type, t => t.Quantity);
        }

        public async Task<bool> VerifyTokenAsync(string userId, string tokenType, int requiredQuantity)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return false;

            var token = wallet.QuotaTokens.FirstOrDefault(t => t.Type == tokenType);
            return token != null && token.Quantity >= requiredQuantity;
        }
    }
}