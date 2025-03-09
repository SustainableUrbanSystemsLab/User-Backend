using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Urbano_API.Models;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using Urbano_API.DTOs;
using System.Security.Claims;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")] // Only users with the "Admin" role can access this controller
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
    
    [Authorize]
    [HttpGet("user/role-get/{username}")]
    public async Task<IActionResult> GetUserRole(string username)
    {
        var user = await _userRepository.GetUserAsync(username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new { Role = user.Role });
    }

    [HttpPut("user/role-set")]
    public async Task<IActionResult> SetUserRole([FromBody] SetUserRoleRequest request)
    {

        var user = await _userRepository.GetUserAsync(request.UserName);
        if (user == null)
        {
            return NotFound("User doesn't exist");
        }

        user.Role = request.NewRole;

        // Update the user's role
        await _userRepository.UpdateAsync(user.Id, user);
        return Ok("Succesfully updated");
    }

    [HttpPut("user/deactivate")]
    public async Task<IActionResult> DeactivateUser([FromBody] DeactivateRequest request)
    {
        var user = await _userRepository.GetUserAsync(request.UserName);
        if (user == null)
        {
            return NotFound("User doesn't exist");
        }

        user.Deactivated = request.Deactivated;

        // Update the user's deactivation status
        await _userRepository.UpdateAsync(user.Id, user);
        return Ok("Succesfully updated");
    }
}