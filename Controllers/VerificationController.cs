using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Urbano_API.Services;
using System.IdentityModel.Tokens.Jwt;


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
                return Ok();
            }
        }

        ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
        return Unauthorized(ModelState);
    }
}

