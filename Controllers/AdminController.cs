using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Urbano_API.Services;
using Urbano_API.Models;
namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController: ControllerBase
{
    private readonly AuthService _authService;

    public AdminController(AuthService authService)
	{
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRateLimit([FromBody] string userName, string token, int maxAttempts)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

        if(role != Roles.ADMIN.ToString())
        {
            ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
            return Unauthorized(ModelState);
        }

        var name = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;
        
        var admin = await _authService.GetUserAsync(name);
        if (admin is null)
        {
            return BadRequest("User doesn't exist");
        }

        var user = await _authService.GetUserAsync(userName);

        if (user == null)
        {
            ModelState.AddModelError("Unauthorized", "User doesn't exist");
            return Unauthorized(ModelState);
        }

        user.maxAttempts = maxAttempts;
        user.attemptsLeft = maxAttempts;

    
        await _authService.UpdateAsync(user.Id, user);

        return Ok("Succesfully updated");
    }

}


