using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

using Urbano_API.DTOs;
using Urbano_API.Models;
using Urbano_API.Interfaces;
using System.Security.Cryptography;
using System;

namespace Urbano_API.Controllers;

[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly IAuthService _authService;
    private readonly IVerificationService _verificationService;
    private readonly IUserRepository _userRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IMetricsRepository _metricsRepository;
     private readonly IWalletRepository _walletRepository;
    private readonly ILoginsRepository _loginsRepository;

    public AuthController(IConfiguration configuration, IAuthService authService, IVerificationService verificationService, IUserRepository userRepository, IVerificationRepository verificationRepository, IMetricsRepository metricsRepository, IWalletRepository walletRepository, ILoginsRepository loginsRepository)
    {
        this.configuration = configuration;
        _authService = authService;
        _verificationService = verificationService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _metricsRepository = metricsRepository;
        _walletRepository = walletRepository;
        _loginsRepository = loginsRepository;
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO credential)
    {
        var resp = await _userRepository.GetUserAsync(credential.UserName);

        var pastLoginDate = resp.LastLoginDate;

        if (resp is null)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        var expiresAt = DateTime.UtcNow.AddMonths(1);

        // Verify the credential
        if (resp.Verified == true && resp.UserName == credential.UserName && CryptographicOperations.FixedTimeEquals(Convert.FromHexString(_authService.GeneratePasswordHash(credential.Password)), Convert.FromHexString(resp.Password)))
        {
            //Update User Login Date
            await _userRepository.UpdateLastLoginDateAsync(resp.Id, DateTime.UtcNow);
            // Creating the security context
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, credential.UserName),
                    new Claim(ClaimTypes.Role, resp.Role),
                };

            // Update SuccessfulLogins counter
            var updatedMetric = await _metricsRepository.IncrementMetricsValueAsync("SuccessfulLogins", 1);
            if (updatedMetric == null)
            {
                // TODO: Handle missing metric initialization.
            }
            // Update Successful Logins Counter Daily
            var updatedLoginDaily = await _loginsRepository.IncrementLoginsDailyValueAsync(DateTime.UtcNow, 1);
            if (updatedLoginDaily == null)
            {
                // TODO: Handle missing login. daily counter.
            }
            // Update Successful. Unique Logins Counter Daily
            var updatedUniqueLoginDaily = await _loginsRepository.IncrementUniqueLoginsDailyValueAsync(DateTime.UtcNow, pastLoginDate,  1);
            if (updatedUniqueLoginDaily == null)
            {
                // TODO: Handle missing login. daily counter.
                // Will indicate that User already logged in on this day
            }

            // Update Successful Logins Counter Weekly
            var updatedLoginWeekly = await _loginsRepository.IncrementLoginsWeeklyValueAsync(DateTime.UtcNow, 1);
            if (updatedLoginWeekly == null)
            {
                // TODO: Handle missing login. Weekly counter.
            }
            // Update Successful. Unique Logins Counter Weekly
            var updatedUniqueLoginWeekly = await _loginsRepository.IncrementUniqueLoginsWeeklyValueAsync(DateTime.UtcNow, pastLoginDate,  1);
            if (updatedUniqueLoginWeekly == null)
            {
                // TODO: Handle missing login. weekly counter.
                // Will indicate that User already logged in on this week
            }

            // Update Successful Logins Counter Monthly
            var updatedLoginMonthly = await _loginsRepository.IncrementLoginsMonthlyValueAsync(DateTime.UtcNow, 1);
            if (updatedLoginMonthly == null)
            {
                // TODO: Handle missing login. Monthly counter.
            }

            // Update Successful. Unique Logins Counter Monthly
            var updatedUniqueLoginMonthly = await _loginsRepository.IncrementUniqueLoginsMonthlyValueAsync(DateTime.UtcNow, pastLoginDate,  1);
            if (updatedUniqueLoginMonthly == null)
            {
                // TODO: Handle missing login. monthly counter.
                // Will indicate that User already logged in on this month
            }

            // Update Successful Logins Counter Yearly
            var updatedLoginYearly = await _loginsRepository.IncrementLoginsYearlyValueAsync(DateTime.UtcNow, 1);
            if (updatedLoginYearly == null)
            {
                // TODO: Handle missing login. Yearly counter.
            }
            // Update Successful. Unique Logins Counter Yearly
            var updatedUniqueLoginYearly = await _loginsRepository.IncrementUniqueLoginsYearlyValueAsync(DateTime.UtcNow, pastLoginDate,  1);
            if (updatedUniqueLoginYearly == null)
            {
                // TODO: Handle missing login. yearly counter.
                // Will indicate that User already logged in on this year
            }

            return Ok(new
            {
                access_token = _verificationService.CreateToken(claims, expiresAt),
                expires_at = expiresAt.Truncate(TimeSpan.FromSeconds(1))
            });
        }

        ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
        return Unauthorized(ModelState);
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
    {
        User user = userDTO.GetUser();
        if (!_authService.IsValidUserName(user.UserName))
        {
            return BadRequest("Incorrect mail Id");
        }
        var resp = await _userRepository.GetUserAsync(user.UserName);

        if (resp is null)
        {
            user.Password = _authService.GeneratePasswordHash(user.Password);
            await _userRepository.CreateAsync(user);
            //_verificationService.SendVerificationMail(user.UserName, user.FirstName + " " + user.LastName);

            // Create a Wallet for New User
            var existingWallet = await _walletRepository.GetWalletByUserIdAsync(user.Id!);
            if (existingWallet is null)
            {
                Wallet wallet = new Wallet(user.Id!)
                {
                    QuotaTokens = new List<QuotaToken>
                    {
                        new QuotaToken { Type = "DefaultToken", Quantity = 0 }
                    }
                };
                await _walletRepository.CreateAsync(wallet);
        }

            return Ok("User Succesfully created");
        }
        else if (resp.Verified is false)
        {
            resp.FirstName = user.FirstName;
            resp.LastName = user.LastName;
            resp.Password = _authService.GeneratePasswordHash(user.Password);
            await _userRepository.UpdateAsync(resp.Id, resp);
            //_verificationService.SendVerificationMail(user.UserName, user.FirstName + " " + user.LastName);
            return Ok("User Succesfully created");
        }
        return BadRequest("User already exists");
    }

    [HttpPost("/otp/generate")]
    public async Task<IActionResult> GenerateOTP([FromBody] OTPDTO emailObj)
    {
        if (!_authService.IsValidUserName(emailObj.UserName))
        {
            return BadRequest("Incorrect e-mail address.");
        }
        var resp = await _userRepository.GetUserAsync(emailObj.UserName);
        if (resp is null)
        {
            return BadRequest("Incorrect e-mail address");
        }

        _verificationService.SendOTP(resp.UserName, resp.FirstName);

        return Ok("Password change request sent to mail");
    }

    [HttpPost("/otp/verify")]
    public async Task<IActionResult> VerifyOTP([FromBody] UserVerificationDTO verUser)
    {
        var user = await _verificationRepository.GetUserAsync(verUser.UserName);
        if (user is null)
        {
            return BadRequest("User doesn't exist");
        }

        if (user.OTP != verUser.OTP)
        {
            ModelState.AddModelError("Unauthorized", "Incorrect OTP");
            return Unauthorized(ModelState);
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, verUser.UserName),
                };
        string token = _verificationService.CreateToken(claims, expiresAt);

        return Ok(token);
    }

    [HttpGet("/verify/{token}")]
    public async Task<IActionResult> Verify(string token)
    {
        var resp = _verificationService.Verify(token);
        if (!resp)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

        var user = await _userRepository.GetUserAsync(userName);
        if (user != null)
        {
            user.Verified = true;
            if (user.Id != null)
            {
                await _userRepository.UpdateAsync(user.Id, user);
                string url = $"{configuration.GetValue<string>("UiURL")}/Login";
                return Redirect(url);
            }
        }

        ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
        return Unauthorized(ModelState);
    }

    [HttpPut("/password")]
    public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO verPass)
    {
        var resp = _verificationService.Verify(verPass.Token);
        if (!resp)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(verPass.Token);
        var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

        var user = await _userRepository.GetUserAsync(userName);
        if (user is null)
        {
            return BadRequest("User doesn't exist");
        }

        user.Password = _authService.GeneratePasswordHash(verPass.Password);
        await _userRepository.UpdateAsync(user.Id, user);

        return Ok("User Succesfully created");
    }
}