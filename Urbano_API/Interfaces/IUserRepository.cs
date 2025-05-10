using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface IUserRepository
{
    public Task<List<User>> GetAsync();

    public Task<User?> GetAsync(string id);

    public Task<User?> GetUserAsync(string userName);

    public Task CreateAsync(User user);

    public Task UpdateAsync(string id, User user);

    public Task RemoveAsync(string id);

    public Task UpdateLastLoginDateAsync(string id, DateTime lastLoginDate);

    public Task SetMigratedFlagAsync(string id, bool isMigrated);

    public Task AddCommunityIfNotExistsAsync(string userId, CommunityType community);

    public Task RemoveCommunityAsync(string userId, CommunityType community);
}