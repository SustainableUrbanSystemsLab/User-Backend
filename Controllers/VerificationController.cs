using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Urbano_API.Services;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;


namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class VerificationController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly VerificationService _verificationService;

    public VerificationController(AuthService authService, VerificationService verificationService)
    {
        _authService = authService;
        _verificationService = verificationService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Verify(string token)
    {
        var resp = _verificationService.Verify(token);
        if(!resp)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

        var user = await _authService.GetUserAsync(userName);
        if(user != null)
        {
            user.Verified = true;
            if(user.Id != null)
            {
                await _authService.UpdateAsync(user.Id, user);

                return Redirect("http://localhost:5173/login");
            }
        }

        ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
        return Unauthorized(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOTP([FromBody] VerUser verUser)
    {

        var user = await _verificationService.GetUserAsync(verUser.UserName);
        if (user is null)
        {
            return BadRequest("User doesn't exist");
        }

        if(user.OTP != verUser.OTP)
        {
            ModelState.AddModelError("Unauthorized", "Incorrect OTP");
            return Unauthorized(ModelState);
        }

        var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, verUser.UserName),
                };
        string token = _verificationService.CreateToken(claims);

        return Ok(token);
    }

    public class VerUser
    {
        public string OTP { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}

