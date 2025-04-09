using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

using Urbano_API.Models;
using Urbano_API.Interfaces;
using System;

namespace Urbano_API.Services;

public class VerificationService : IVerificationService
{
    private static string emailVerificationBody = File.ReadAllText("Assets/EmailVerificationBody.html");
    private static string otpVerificationBody = File.ReadAllText("Assets/PasswordChangeOTPBody.html");

    private readonly IConfiguration configuration;
    private readonly IVerificationRepository _verificationRepository;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public VerificationService(IConfiguration configuration, IOptions<UrbanoStoreEmailSettings> urbanoStoreEmailSettings, IVerificationRepository verificationRepository)
    {
        this.configuration = configuration;
        _smtpServer = urbanoStoreEmailSettings.Value.SmtpServer;
        _smtpPort = urbanoStoreEmailSettings.Value.SmtpPort;
        _smtpUsername = urbanoStoreEmailSettings.Value.SmtpUsername;
        _smtpPassword = urbanoStoreEmailSettings.Value.SmtpPassword;
        _fromEmail = urbanoStoreEmailSettings.Value.FromEmail;
        _fromName = urbanoStoreEmailSettings.Value.FromName;
        _verificationRepository = verificationRepository;
    }

    public void SendVerificationMail(string email, string name)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Email, email),
        };

        string url = $"{configuration.GetValue<string>("ApiURL")}/verify/{CreateToken(claims, DateTime.UtcNow.AddMinutes(10))}";
        var htmlContent = emailVerificationBody.Replace("{VerificationUrl}", url);

        SendEmail(email, name, "Verify your email address", htmlContent);
    }

    public void SendOTP(string email, string name, string purpose)
    {
        if (string.IsNullOrEmpty(purpose))
        {
            throw new ArgumentException("Purpose cannot be null or empty.");
        }

        if (purpose != "password-reset" && purpose != "email-change")
        {
            throw new ArgumentException("Invalid purpose. Allowed values: 'password-reset', 'email-change'.");
        }

        var otp = (OTPGeneratorService.NextInt() % 10000).ToString("0000");
        var otpExpiry = DateTime.UtcNow.AddMinutes(10);
        
        _verificationRepository.UpsertAsync(otp, email, otpExpiry).Wait();

        string subject = purpose == "email-change" ? "Verify your email change" : "Verify your password reset";
        string body = purpose == "email-change"
            ? $"Hello {name}, your OTP for email change is: {otp}"
            : $"Hello {name}, your OTP for password reset is: {otp}";

        SendEmail(email, name, subject, body);
    }

    private void SendEmail(string toEmail, string toName, string subject, string htmlContent)
    {
        try
        {
            Console.WriteLine($"Attempting to send email to {toEmail}...");
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true; // Enable SSL/TLS

                var from = new MailAddress(_fromEmail, _fromName);
                var to = new MailAddress(toEmail, toName);

                var message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };

                client.Send(message);
                Console.WriteLine($"Email sent successfully to {toEmail}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    public bool Verify(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // Change from ASCII to UTF8 to match token creation
        var secretKey = Encoding.UTF8.GetBytes(configuration["SecretKey"] ?? "");

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ClockSkew = TimeSpan.Zero,
            };
            SecurityToken validatedToken;
            IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public string CreateToken(IEnumerable<Claim> claims, DateTime expireAt)
    {
        // Get key from config and trim any whitespace
        var secretKey = Encoding.UTF8.GetBytes(configuration["SecretKey"]?.Trim() ?? throw new Exception("SecretKey is missing"));
        
        var jwt = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expireAt,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256) // Note: Using HmacSha256 (not HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public static string CreateToken(IConfiguration configuration, IEnumerable<Claim> claims, DateTime expireAt)
    {
        // Get key from config and trim any whitespace
        var secretKey = Encoding.UTF8.GetBytes(configuration["SecretKey"]?.Trim() ?? throw new Exception("SecretKey is missing"));
        
        var jwt = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expireAt,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256) // Note: Using HmacSha256 (not HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}