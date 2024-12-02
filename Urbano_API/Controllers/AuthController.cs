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

    public AuthController(IConfiguration configuration, IAuthService authService, IVerificationService verificationService, IUserRepository userRepository, IVerificationRepository verificationRepository)
    {
        this.configuration = configuration;
        _authService = authService;
        _verificationService = verificationService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _metricsRepository = // TODO
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO credential)
    {
        var resp = await _userRepository.GetUserAsync(credential.UserName);

        if (resp is null)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        var expiresAt = DateTime.UtcNow.AddMonths(1);

        // Verify the credential
        if (resp.Verified == true && resp.UserName == credential.UserName && CryptographicOperations.FixedTimeEquals(Convert.FromHexString(_authService.GeneratePasswordHash(credential.Password)), Convert.FromHexString(resp.Password)))
        {
            // Creating the security context
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, credential.UserName),
                    new Claim(ClaimTypes.Role, resp.Role),
                };

            // Update SuccessfulLogins counter
            _authService.IncrementLoginCounter(/* TODO: get the correct metrics object */)

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