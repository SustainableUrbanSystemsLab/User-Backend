using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Urbano_API.Models;
using static Urbano_API.Controllers.LoginController;

namespace Urbano_API.Services
{
	public class VerificationService
	{
        private readonly IConfiguration configuration;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;


        public VerificationService(IConfiguration configuration, IOptions<UrbanoStoreEmailSettings> urbanoStoreEmailSettings)
        {
            this.configuration = configuration;
            _apiKey = urbanoStoreEmailSettings.Value.APIKey;
            _fromEmail = urbanoStoreEmailSettings.Value.SenderAddress;
            _fromName = urbanoStoreEmailSettings.Value.SenderName;
        }

        public void sendVerificationMail(string email, string name)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var subject = "Verify your email address";
            var to = new EmailAddress(email, name);
            var plainTextContent = "This email address was recently used to log into Urbano. If this was you, please verify your email address by clicking the following link:\n\n";
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, email),
                };
            string url = $"https://localhost:7054/Verification/{CreateToken(claims)}";
            var htmlContent = $"<a href = {url}>Confirm my account</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            //var response = client.SendEmailAsync(msg);
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
                    ValidateLifetime = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero,
                };
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return true;
            } catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var secretKey = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey") ?? "");

            // generate the JWT
            var jwt = new JwtSecurityToken(
                    claims: claims,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(secretKey),
                        SecurityAlgorithms.HmacSha256Signature)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

    }
}

