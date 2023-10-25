using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using Urbano_API.Models;
using System.Security.Claims;
using Urbano_API.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class ResetPasswordController: ControllerBase
{
    private readonly AuthService _authService;
    private readonly VerificationService _verificationService;

    public ResetPasswordController(AuthService authService, VerificationService verificationService)
	{
        _authService = authService;
        _verificationService = verificationService;
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePassword([FromBody] VerPass verPass)
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

        var user = await _authService.GetUserAsync(userName);
        if (user is null)
        {
            return BadRequest("User doesn't exist");
        }

        user.Password = verPass.Password;
        await _authService.UpdateAsync(user.Id, user);

        return Ok("User Succesfully created");
    }


    public class VerPass
    {
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

