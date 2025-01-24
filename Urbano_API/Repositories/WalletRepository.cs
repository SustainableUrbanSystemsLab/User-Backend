using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Models;
using Urbano_API.Interfaces;

namespace Urbano_API.Repositories;

public class WalletRepository: IWalletRepository
{
    private readonly IMongoCollection<Wallet> _wallets;

    public WalletRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _wallets = mongoDatabase.GetCollection<Wallet>(
            urbanoStoreDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<List<Wallet>> GetAsync() =>
        await _wallets.Find(_ => true).ToListAsync();

    public async Task<Wallet?> GetAsync(string id) =>
        await _wallets.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Wallet?> GetWalletByUserIdAsync(string userId) =>
        await _wallets.Find(x => x.UserId == userId).FirstOrDefaultAsync();

    public async Task CreateAsync(Wallet wallet) =>
        await _wallets.InsertOneAsync(wallet);

    public async Task UpdateAsync(string id, Wallet wallet) =>
        await _wallets.ReplaceOneAsync(x => x.Id == id, wallet);

    public async Task RemoveAsync(string id) =>
        await _wallets.DeleteOneAsync(x => x.Id == id);
}


