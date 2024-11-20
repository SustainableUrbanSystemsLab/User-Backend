using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Urbano_API.Services;
using Urbano_API.Models;
using Urbano_API.Repositories;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController: ControllerBase
{
    private readonly UserRepository _userRepository;

    public AdminController(UserRepository userRepository)
	{
        _userRepository = userRepository;
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
        
        var admin = await _userRepository.GetUserAsync(name);
        if (admin is null)
        {
            return BadRequest("User doesn't exist");
        }

        var user = await _userRepository.GetUserAsync(userName);

        if (user == null)
        {
            ModelState.AddModelError("Unauthorized", "User doesn't exist");
            return Unauthorized(ModelState);
        }

        user.MaxAttempts = maxAttempts;
        user.AttemptsLeft = maxAttempts;

    
        await _userRepository.UpdateAsync(user.Id, user);

        return Ok("Succesfully updated");
    }

}


