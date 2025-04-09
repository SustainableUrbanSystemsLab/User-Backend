using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Urbano_API.Interfaces;
using Urbano_API.Models;

namespace Urbano_API.Services;

public class AuthService: IAuthService
{
    const int keySize = 64;
    const int iterations = 350000;
    HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
    private readonly IConfiguration configuration;

    public AuthService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public bool IsValidUserName(string userName)
    {
        var valid = true;

        try
        {
            var emailAddress = new MailAddress(userName);
        }
        catch
        {
            valid = false;
        }

        return valid;
    }

    public string GeneratePasswordHash(string password)
    {
        var saltString = configuration["passwordSalt"] ?? 
                        throw new Exception("Password salt is not configured");
        
        // Use UTF8 encoding consistently (matches JWT token handling)
        var salt = Encoding.UTF8.GetBytes(saltString);
    
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            hashAlgorithm,
            keySize);

        return Convert.ToHexString(hash);
    }
}

