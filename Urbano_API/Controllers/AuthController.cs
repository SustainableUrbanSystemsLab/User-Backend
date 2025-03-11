using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

using Urbano_API.DTOs;
using Urbano_API.Models;
using Urbano_API.Interfaces;
using System.Security.Cryptography;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

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
    private readonly IRegistrationsRepository _registrationsRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ISimulationsRepository _simulationsRepository;
    private readonly ILoginsRepository _loginsRepository;

    public AuthController(IConfiguration configuration, IAuthService authService, IVerificationService verificationService, IUserRepository userRepository, IVerificationRepository verificationRepository, IMetricsRepository metricsRepository, IRegistrationsRepository registrationsRepository, IWalletRepository walletRepository, ILoginsRepository loginsRepository, ISimulationsRepository simulationsRepository)
    {
        private readonly IConfiguration configuration;
        private readonly IAuthService _authService;
        private readonly IVerificationService _verificationService;
        private readonly IUserRepository _userRepository;
        private readonly IVerificationRepository _verificationRepository;
        private readonly IMetricsRepository _metricsRepository;
        private readonly IRegistrationsRepository _registrationsRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ISimulationsRepository _simulationsRepository;
        private readonly ILoginsRepository _loginsRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration, 
            IAuthService authService, 
            IVerificationService verificationService, 
            IUserRepository userRepository, 
            IVerificationRepository verificationRepository, 
            IMetricsRepository metricsRepository, 
            IRegistrationsRepository registrationsRepository, 
            IWalletRepository walletRepository, 
            ILoginsRepository loginsRepository, 
            ISimulationsRepository simulationsRepository,
            ILogger<AuthController> logger)
        {
            this.configuration = configuration;
            _authService = authService;
            _verificationService = verificationService;
            _userRepository = userRepository;
            _verificationRepository = verificationRepository;
            _metricsRepository = metricsRepository;
            _registrationsRepository = registrationsRepository;
            _simulationsRepository = simulationsRepository;
            _walletRepository = walletRepository;
            _loginsRepository = loginsRepository;
            _logger = logger;
        }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO credential)
    {
        try
        {
            var resp = await _userRepository.GetAsync(credential.UserId);

            // This line might throw a NullReferenceException if resp is null.
            var pastLoginDate = resp.LastLoginDate;

            // User does not exist
            if (resp is null)
            {
                ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
                return Unauthorized(ModelState);
            }

            // User's account was previously deactivated
            if (resp.Deactivated == true)
            {
                ModelState.AddModelError("Unauthorized", "Your account has been deactivated.");
                return Unauthorized(ModelState);
            }

            var expiresAt = DateTime.UtcNow.AddMonths(1);

            // Verify the credential
            if (resp.Verified == true && resp.UserId == credential.UserId && CryptographicOperations.FixedTimeEquals(Convert.FromHexString(_authService.GeneratePasswordHash(credential.Password)), Convert.FromHexString(resp.Password)))
            {
                //Update User Login Date
                await _userRepository.UpdateLastLoginDateAsync(resp.Id!, DateTime.UtcNow);
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
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while processing the login request: " + ex.Message);
        }
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
    {
        try
        {
            User user = userDTO.GetUser();
            if (!_authService.IsValidUserName(user.UserName))
            {
                return BadRequest("Incorrect mail Id");
            }
            var resp = await _userRepository.GetUserAsync(user.UserName);

            // New User
            if (resp is null)
            {
                user.Password = _authService.GeneratePasswordHash(user.Password);
                await _userRepository.CreateAsync(user);
                _verificationService.SendVerificationMail(user.UserName, user.FirstName + " " + user.LastName);

                // Update all temporal registrations counters
                var updatedRegistrationDaily = await _registrationsRepository.IncrementRegistrationsDailyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationDaily == null)
                {
                    // TODO: Handle missing registration initialization.
                }
                var updatedRegistrationWeekly = await _registrationsRepository.IncrementRegistrationsWeeklyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationWeekly == null)
                {
                    // TODO: Handle missing registration initialization.
                }
                var updatedRegistrationMonthly = await _registrationsRepository.IncrementRegistrationsMonthlyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationMonthly == null)
                {
                    // TODO: Handle missing registration initialization.
                }
                var updatedRegistrationYearly = await _registrationsRepository.IncrementRegistrationsYearlyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationYearly == null)
                {
                    // TODO: Handle missing registration initialization.
                }

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
            // Unverified User
            else if (resp.Verified is false)
            {
                resp.FirstName = user.FirstName;
                resp.LastName = user.LastName;
                resp.Password = _authService.GeneratePasswordHash(user.Password);
                await _userRepository.UpdateAsync(resp.Id, resp);
                _verificationService.SendVerificationMail(user.UserName, user.FirstName + " " + user.LastName);

                // Update all temporal registrations counters
                var updatedRegistrationDaily = await _registrationsRepository.IncrementRegistrationsDailyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationDaily == null)
                {
                    // TODO: Handle missing registration initialization.
                }
                var updatedRegistrationWeekly = await _registrationsRepository.IncrementRegistrationsWeeklyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationWeekly == null)
                {
                    resp.FirstName = user.FirstName;
                    resp.LastName = user.LastName;
                    resp.Password = _authService.GeneratePasswordHash(user.Password);
                    await _userRepository.UpdateAsync(resp.Id!, resp);
                    _verificationService.SendVerificationMail(user.UserName, $"{user.FirstName} {user.LastName}");
                    // TODO: Handle missing registration initialization.
                }
                var updatedRegistrationMonthly = await _registrationsRepository.IncrementRegistrationsMonthlyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationMonthly == null)
                {
                    // TODO: Handle missing registration initialization.
                }
                var updatedRegistrationYearly = await _registrationsRepository.IncrementRegistrationsYearlyValueAsync(DateTime.UtcNow, 1);
                if (updatedRegistrationYearly == null)
                {
                    // TODO: Handle missing registration initialization.
                }

                return Ok("User Succesfully created");
            }
            return BadRequest("User already exists");
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while processing the registration request: " + ex.Message);
        }
    }
    [HttpGet("daily-count/{userId}")]
    public async Task<IActionResult> GetUserDailyLoginCount(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required.");
            }

            var count = await _loginsRepository.GetUserDailyLoginCountAsync(userId);
            return Ok(new { userId, totalDailyLogins = count });
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while retrieving the daily login count: " + ex.Message);
        }
    }

    [HttpPost("/simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulationsDTO simulationDTO)
    {
        try
        {
            if (simulationDTO == null || simulationDTO.UserId == null || string.IsNullOrEmpty(simulationDTO.TokenType))
            {
                return BadRequest("Invalid simulation request.");
            }
            var tokenType = simulationDTO.TokenType;

            // Retrieve the wallet
            var wallet = await _walletRepository.GetWalletByUserIdAsync(simulationDTO.UserId);
            if (wallet == null)
            {
                return BadRequest("Wallet not found for the user.");
            }

            // Check if the wallet has the simulation token
            QuotaToken? token = wallet.QuotaTokens
                    .Where(qt => qt.Type.Equals(tokenType, StringComparison.OrdinalIgnoreCase) && qt.Quantity >= 1)
                    .SingleOrDefault();
            if (token == null)
            {
                return BadRequest($"Insufficient '{tokenType}' tokens in the wallet.");
            }

            // Success
            /********************************************************************************************
            // TODO: below line doesn't actually work
            // token.Quantity -= 1;
            **/
            var success = await _walletRepository.RemoveTokenAsync(simulationDTO.UserId, tokenType, 1);
            var daily = await _simulationsRepository.IncrementSimulationsDailyValueAsync(DateTime.UtcNow, 1, tokenType);
            var weekly = await _simulationsRepository.IncrementSimulationsWeeklyValueAsync(DateTime.UtcNow, 1, tokenType);
            var monthly = await _simulationsRepository.IncrementSimulationsMonthlyValueAsync(DateTime.UtcNow, 1, tokenType);
            var yearly = await _simulationsRepository.IncrementSimulationsYearlyValueAsync(DateTime.UtcNow, 1, tokenType);

            return Ok("Simulation successfully executed.");
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while processing the simulation request: " + ex.Message);
        }
    }

    [HttpGet("count/{userId}")]
    public async Task<IActionResult> GetUserSimulationCount(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required.");
            }

            var count = await _simulationsRepository.GetUserSimulationCountAsync(userId);
            return Ok(new { userId, totalSimulations = count });
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while retrieving the simulation count: " + ex.Message);
        }
    }

    [HttpPost("/otp/generate")]
    public async Task<IActionResult> GenerateOTP([FromBody] OTPDTO emailObj, [FromQuery] string purpose)
    {
        try
        {
            if (string.IsNullOrEmpty(purpose))
            {
                return BadRequest("Purpose is required.");
            }

            if (purpose != "password-reset" && purpose != "email-change")
            {
                return BadRequest("Invalid purpose. Allowed values: 'password-reset', 'email-change'.");
            }

            if (!_authService.IsValidUserName(emailObj.UserName))
            {
                return BadRequest("Incorrect e-mail address.");
            }

            var user = await _userRepository.GetUserAsync(emailObj.UserName);
            if (user is null)
            {
                return BadRequest("User doesn't exist");
            }

            _verificationService.SendOTP(user.UserName, user.FirstName, purpose);

            return Ok($"{purpose} OTP sent to email");
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while generating the OTP: " + ex.Message);
        }
    }

    [HttpPost("/otp/verify")]
    public async Task<IActionResult> VerifyOTP([FromBody] UserVerificationDTO verUser, [FromQuery] string purpose)
    {
        try
        {
            if (string.IsNullOrEmpty(purpose))
            {
                return BadRequest("Purpose is required.");
            }

            if (purpose != "password-reset" && purpose != "email-change")
            {
                return BadRequest("Invalid purpose. Allowed values: 'password-reset', 'email-change'.");
            }

            var verification = await _verificationRepository.GetUserAsync(verUser.UserName);
            if (verification is null)
            {
                return BadRequest("User doesn't exist");
            }

            if (verification.OTP != verUser.OTP || verification.OTPExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("Unauthorized", "Incorrect or expired OTP");
                return Unauthorized(ModelState);
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Email, verUser.UserName),
                new Claim("Purpose", purpose) // Add a claim to indicate the purpose
            };

            string token = _verificationService.CreateToken(claims, expiresAt);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while verifying the OTP: " + ex.Message);
        }
    }

    [HttpGet("/verify/{token}")]
    public async Task<IActionResult> Verify(string token)
    {
        try
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
                    string url = $"{configuration.GetValue<string>("ApiURL")}/Login";   // IS POINTING TO BACKEND FOR LOCAL DEV RN!!!!!!!!!!!!!!!!
                    return Redirect(url);
                }
            }

            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred during verification: " + ex.Message);
        }
    }

    [HttpPut("/password")]
    public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO verPass)
    {
        try
        {
            var isValidToken = _verificationService.Verify(verPass.Token);
            if (!isValidToken)
            {
                ModelState.AddModelError("Unauthorized", "Invalid or expired token.");
                return Unauthorized(ModelState);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(verPass.Token);
            var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            // Verify the token's purpose
            var purpose = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "Purpose")?.Value;
            if (purpose != "password-reset")
            {
                ModelState.AddModelError("Unauthorized", "Invalid token purpose.");
                return Unauthorized(ModelState);
            }

            var user = await _userRepository.GetUserAsync(userName);
            if (user is null)
            {
                return BadRequest("User doesn't exist");
            }

            user.Password = _authService.GeneratePasswordHash(verPass.Password);
            await _userRepository.UpdateAsync(user.Id, user);

            return Ok("Password successfully updated");
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while updating the password: " + ex.Message);
        }
    }

    [HttpPut("/email")]
    public async Task<IActionResult> UpdateEmail([FromBody] EmailChangeDTO emailChangeDTO)
    {
        try
        {
            var isValidToken = _verificationService.Verify(emailChangeDTO.Token);
            if (!isValidToken)
            {
                ModelState.AddModelError("Unauthorized", "Invalid or expired token.");
                return Unauthorized(ModelState);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(emailChangeDTO.Token);
            var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            // Verify the token's purpose
            var purpose = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "Purpose")?.Value;
            if (purpose != "email-change")
            {
                ModelState.AddModelError("Unauthorized", "Invalid token purpose.");
                return Unauthorized(ModelState);
            }

            var user = await _userRepository.GetUserAsync(userName);
            if (user is null)
            {
                return BadRequest("User doesn't exist");
            }

            // Validate the new email
            if (!_authService.IsValidUserName(emailChangeDTO.NewEmail))
            {
                return BadRequest("Invalid new email address.");
            }

            // Check if the new email is already in use
            var existingUser = await _userRepository.GetUserAsync(emailChangeDTO.NewEmail);
            if (existingUser != null)
            {
                return BadRequest("Email address is already in use.");
            }

            // Update the email
            user.UserName = emailChangeDTO.NewEmail;
            await _userRepository.UpdateAsync(user.Id, user);

            return Ok("Email successfully updated");
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, "An error occurred while updating the email: " + ex.Message);
        }
    }
}
