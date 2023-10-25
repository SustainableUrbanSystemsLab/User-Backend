using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Urbano_API.Services;
using static Urbano_API.Controllers.ResetPasswordController;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly VerificationService _verificationService;

    public HomeController(AuthService authService, VerificationService verificationService)
    {
        _authService = authService;
        _verificationService = verificationService;
    }

    [HttpPost]
    public async Task<IActionResult> HomePage([FromBody] string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

        var user = await _authService.GetUserAsync(userName);

        if(user == null)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        if (user.Date.CompareTo(DateTime.Today.AddHours(-24)) <= 0) {
            user.attemptsLeft = user.maxAttempts;
            user.Date = DateTime.Now;
            await _authService.UpdateAsync(user.Id, user);
        } else if(user.attemptsLeft == 0)
        {
            ModelState.AddModelError("Unauthorized", "Request Limit Reached");
            return Unauthorized(ModelState);
        }

        user.attemptsLeft -= 1;
        await _authService.UpdateAsync(user.Id, user);

        return Ok("Succesfully logged in");
    }
}


