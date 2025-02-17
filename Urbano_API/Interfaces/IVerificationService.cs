using System.Security.Claims;

namespace Urbano_API.Interfaces;

public interface IVerificationService
{
    public void SendVerificationMail(string email, string name);

    public void SendOTP(string email, string name, string purpose);

    public bool Verify(string token);

    public string CreateToken(IEnumerable<Claim> claims, DateTime expireAt);
}

