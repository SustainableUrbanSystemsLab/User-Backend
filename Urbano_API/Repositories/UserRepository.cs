﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Urbano_API.Interfaces;
using Urbano_API.Models;

namespace Urbano_API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserRepository(IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            urbanoStoreDatabaseSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(
            urbanoStoreDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetUserAsync(string userName) =>
        await _usersCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();

    public async Task CreateAsync(User user) =>
        await _usersCollection.InsertOneAsync(user);

    public async Task UpdateAsync(string id, User user) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, user);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);

    public async Task UpdateLastLoginDateAsync(string userId, DateTime lastLoginDate)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.LastLoginDate, lastLoginDate);

        try
        {
            await _usersCollection.UpdateOneAsync(filter, update);
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }

    public async Task SetMigratedFlagAsync(string userId, bool isMigrated)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.MigratedUser, isMigrated);

        try
        {
            await _usersCollection.UpdateOneAsync(filter, update);
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error while setting MigratedUser flag: {ex.Message}");
            throw;
        }
    }

    public async Task AddCommunityIfNotExistsAsync(string userId, CommunityType community)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.Not(
                Builders<User>.Filter.AnyEq(u => u.Communities, community)
            )
        );

        var update = Builders<User>.Update.Push(u => u.Communities, community);

        try
        {
            await _usersCollection.UpdateOneAsync(filter, update);
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error while adding community: {ex.Message}");
            throw;
        }
    }

    public async Task RemoveCommunityAsync(string userId, CommunityType community)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Pull(u => u.Communities, community);

        try
        {
            await _usersCollection.UpdateOneAsync(filter, update);
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"MongoDB error while removing community: {ex.Message}");
            throw;
        }
    }
}