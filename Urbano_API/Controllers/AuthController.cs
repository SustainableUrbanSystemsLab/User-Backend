using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Urbano_API.DTOs;
using Urbano_API.Models;
using Urbano_API.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Urbano_API.Controllers
{
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
                var resp = await _userRepository.GetUserAsync(credential.UserName);
                if (resp is null)
                {
                    ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
                    return Unauthorized(ModelState);
                }

                if (resp.Deactivated == true)
                {
                    ModelState.AddModelError("Unauthorized", "Your account has been deactivated.");
                    return Unauthorized(ModelState);
                }

                var pastLoginDate = resp.LastLoginDate;
                var expiresAt = DateTime.UtcNow.AddMonths(1);

                if (resp.Verified == true &&
                    resp.UserName == credential.UserName &&
                    CryptographicOperations.FixedTimeEquals(
                        Convert.FromHexString(_authService.GeneratePasswordHash(credential.Password)),
                        Convert.FromHexString(resp.Password)))
                {
                    await _userRepository.UpdateLastLoginDateAsync(resp.Id, DateTime.UtcNow);
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Email, credential.UserName),
                        new Claim(ClaimTypes.Role, resp.Role),
                    };

                    // Various metric updates (omitted for brevity)
                    // ...

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
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "Internal server error");
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

                if (resp is null)
                {
                    user.Password = _authService.GeneratePasswordHash(user.Password);
                    await _userRepository.CreateAsync(user);
                    _verificationService.SendVerificationMail(user.UserName, $"{user.FirstName} {user.LastName}");

                    // Increment registration metrics
                    await _registrationsRepository.IncrementRegistrationsDailyValueAsync(DateTime.UtcNow, 1);
                    await _registrationsRepository.IncrementRegistrationsWeeklyValueAsync(DateTime.UtcNow, 1);
                    await _registrationsRepository.IncrementRegistrationsMonthlyValueAsync(DateTime.UtcNow, 1);
                    await _registrationsRepository.IncrementRegistrationsYearlyValueAsync(DateTime.UtcNow, 1);

                    // Create wallet if needed
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

                    return Ok("User Successfully created");
                }
                else if (resp.Verified is false)
                {
                    resp.FirstName = user.FirstName;
                    resp.LastName = user.LastName;
                    resp.Password = _authService.GeneratePasswordHash(user.Password);
                    await _userRepository.UpdateAsync(resp.Id, resp);
                    _verificationService.SendVerificationMail(user.UserName, $"{user.FirstName} {user.LastName}");

                    await _registrationsRepository.IncrementRegistrationsDailyValueAsync(DateTime.UtcNow, 1);
                    await _registrationsRepository.IncrementRegistrationsWeeklyValueAsync(DateTime.UtcNow, 1);
                    await _registrationsRepository.IncrementRegistrationsMonthlyValueAsync(DateTime.UtcNow, 1);
                    await _registrationsRepository.IncrementRegistrationsYearlyValueAsync(DateTime.UtcNow, 1);

                    return Ok("User Successfully created");
                }
                return BadRequest("User already exists");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, "Internal server error");
            }
        }

        // Additional endpoints (GetUserDailyLoginCount, Simulate, OTP generation/verification, etc.)
        // should be similarly wrapped in try-catch blocks.
        // For brevity, only a couple more examples are shown below.

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
                _logger.LogError(ex, "Error fetching daily login count");
                return StatusCode(500, "Internal server error");
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
                var wallet = await _walletRepository.GetWalletByUserIdAsync(simulationDTO.UserId);
                if (wallet == null)
                {
                    return BadRequest("Wallet not found for the user.");
                }
                var token = wallet.QuotaTokens
                    .FirstOrDefault(qt => qt.Type.Equals(tokenType, StringComparison.OrdinalIgnoreCase) && qt.Quantity >= 1);
                if (token == null)
                {
                    return BadRequest($"Insufficient '{tokenType}' tokens in the wallet.");
                }
                var success = await _walletRepository.RemoveTokenAsync(simulationDTO.UserId, tokenType, 1);
                await _simulationsRepository.IncrementSimulationsDailyValueAsync(DateTime.UtcNow, 1, tokenType);
                await _simulationsRepository.IncrementSimulationsWeeklyValueAsync(DateTime.UtcNow, 1, tokenType);
                await _simulationsRepository.IncrementSimulationsMonthlyValueAsync(DateTime.UtcNow, 1, tokenType);
                await _simulationsRepository.IncrementSimulationsYearlyValueAsync(DateTime.UtcNow, 1, tokenType);
                return Ok("Simulation successfully executed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during simulation execution");
                return StatusCode(500, "Internal server error");
            }
        }

        // Other endpoints (GenerateOTP, VerifyOTP, Verify, UpdatePassword, UpdateEmail) should follow a similar pattern.
    }
}

