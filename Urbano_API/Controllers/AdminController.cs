using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Urbano_API.Services;
using Urbano_API.Models;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using Urbano_API.DTOs;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController: ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository)
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

    [HttpGet("user/role/{username}")]
    public async Task<IActionResult> GetUserRole(string username)
    {
        var user = await _userRepository.GetUserAsync(username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new { Role = user.Role });
    }
    [HttpPut("user/role")]
    public async Task<IActionResult> SetUserRole([FromBody] UserRoleDTO userRoleDTO)
    {
        var user = await _userRepository.GetUserAsync(userRoleDTO.UserName);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Update the user's role
        user.Role = userRoleDTO.Role;
        await _userRepository.UpdateAsync(user.Id, user);

        return Ok($"User {userRoleDTO.UserName} role updated to {userRoleDTO.Role}");
    }

}


