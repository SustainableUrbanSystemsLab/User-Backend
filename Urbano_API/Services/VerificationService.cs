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

public class VerificationService: IVerificationService
{
    private static string emailVerificationBody = File.ReadAllText("Assets/EmailVerificationBody.html");
    private static string otpVerificationBody = File.ReadAllText("Assets/PasswordChangeOTPBody.html");

    private readonly IConfiguration configuration;
    private readonly IVerificationRepository _verificationRepository;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;


    public VerificationService(IConfiguration configuration, IOptions<UrbanoStoreEmailSettings> urbanoStoreEmailSettings, IVerificationRepository verificationRepository)
    {
        this.configuration = configuration;
        _apiKey = urbanoStoreEmailSettings.Value.APIKey;
        _fromEmail = urbanoStoreEmailSettings.Value.SenderAddress;
        _fromName = urbanoStoreEmailSettings.Value.SenderName;
        _verificationRepository = verificationRepository;
    }

    public void SendVerificationMail(string email, string name)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var subject = "Verify your email address";
        var to = new EmailAddress(email, name);
        var plainTextContent = "";
        var claims = new List<Claim> {
                new Claim(ClaimTypes.Email, email),
            };
        string url = $"{configuration.GetValue<string>("ApiURL")}/verify/{CreateToken(claims, DateTime.UtcNow.AddMinutes(10))}";
        var htmlContent = emailVerificationBody.Replace("{VerificationUrl}", url);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = client.SendEmailAsync(msg);
    }

    public async void SendOTP(string email, string name)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var subject = "Verify your email address";
        var to = new EmailAddress(email, name);
        var plainTextContent = "Password change request was recently raised on this email address:\n\n";
        var otp = (OTPGeneratorService.NextInt() % 10000).ToString("0000");
        await _verificationRepository.UpsertAsync(otp, email);
        var htmlContent = otpVerificationBody.Replace("{UserName}", name).Replace("{OTP}", otp);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = client.SendEmailAsync(msg);
    }

    public bool Verify(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey") ?? "");

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
        } catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public string CreateToken(IEnumerable<Claim> claims, DateTime expireAt)
    {
        var secretKey = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey") ?? "");

        // generate the JWT
        var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expireAt,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

}

