using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Urbano_API.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace Urbano_API.Services
{
    public class VerificationService
    {
        private static string emailVerificationBody = File.ReadAllText("Assets/EmailVerificationBody.html");
        private static string otpVerificationBody = File.ReadAllText("Assets/PasswordChangeOTPBody.html");

        private readonly IConfiguration configuration;
        private readonly IMongoCollection<Verification> _verificationCollection;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;


        public VerificationService(IConfiguration configuration, IOptions<UrbanoStoreEmailSettings> urbanoStoreEmailSettings, IOptions<UrbanoStoreDatabaseSettings> urbanoStoreDatabaseSettings)
        {
            this.configuration = configuration;
            _apiKey = urbanoStoreEmailSettings.Value.APIKey;
            _fromEmail = urbanoStoreEmailSettings.Value.SenderAddress;
            _fromName = urbanoStoreEmailSettings.Value.SenderName;

            var mongoClient = new MongoClient(
                urbanoStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                urbanoStoreDatabaseSettings.Value.DatabaseName);

            _verificationCollection = mongoDatabase.GetCollection<Verification>(
                urbanoStoreDatabaseSettings.Value.VerificationsCollectionName);
        }

        public void sendVerificationMail(string email, string name)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var subject = "Verify your email address";
            var to = new EmailAddress(email, name);
            var plainTextContent = "";//"This email address was recently used to log into Urbano. If this was you, please verify your email address by clicking the following link:\n\n";
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, email),
                };
            string url = $"{configuration.GetValue<string>("UiURL")}/{CreateToken(claims)}";
            //var htmlContent = $"<a href = {url}>Confirm my account</a>";
            Console.WriteLine(emailVerificationBody);
            Console.WriteLine("---------");
            var htmlContent = emailVerificationBody.Replace("{VerificationUrl}", url);
            Console.WriteLine(htmlContent);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);
        }

        public async void sendOTP(string email, string name)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var subject = "Verify your email address";
            var to = new EmailAddress(email, name);
            var plainTextContent = "Password change request was recently raised on this email address:\n\n";
            var otp = (OTPGeneratorService.NextInt() % 10000).ToString("0000");
            await this.UpsertAsync(otp, email);
            //var htmlContent = $"<div>OTP to validate your account: {otp} </div>";
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
                    ValidateLifetime = false,
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

        public async Task UpsertAsync(string otp, string userName) {
            var resp = await _verificationCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();
            if(resp == null)
            {
                Verification verification = new Verification();
                verification.UserName = userName;
                verification.OTP = otp;
               await _verificationCollection.InsertOneAsync(verification);
            } else
            {
                resp.OTP = otp;
                await _verificationCollection.ReplaceOneAsync(x => x.UserName == userName, resp);
            }
        }

        public async Task<Verification?> GetUserAsync(string userName) => 
            await _verificationCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();
    }
}

