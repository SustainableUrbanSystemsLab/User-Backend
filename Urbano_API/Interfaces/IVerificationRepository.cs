using Urbano_API.Models;

namespace Urbano_API.Interfaces;

public interface IVerificationRepository
{
    public Task UpsertAsync(string otp, string userName);

    public Task<Verification?> GetUserAsync(string userName);
}

