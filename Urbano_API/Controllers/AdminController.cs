using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Urbano_API.Models;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using Urbano_API.DTOs;
using System.Security.Claims;

namespace Urbano_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")] // Only users with the "Admin" role can access this controller
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRateLimit([FromBody] string userName, string token, int maxAttempts)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(userName);

                if (user == null)
                {
                    ModelState.AddModelError("Unauthorized", "User doesn't exist");
                    return Unauthorized(ModelState);
                }

                user.MaxAttempts = maxAttempts;
                user.AttemptsLeft = maxAttempts;

                await _userRepository.UpdateAsync(user.Id!, user);
                return Ok("Successfully updated");
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return StatusCode(500, "An error occurred while updating the rate limit: " + ex.Message);
            }
        }

        [HttpPut("user/role-set")]
        public async Task<IActionResult> SetUserRole([FromBody] SetUserRoleRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(request.UserName);
                if (user == null)
                {
                    return NotFound("User doesn't exist");
                }
                if (user.Role == request.NewRole)
                {
                // Either return a 304 Not Modified or a custom message.
                return StatusCode(304, "No changes detected; user role remains unchanged.");
                }

                user.Role = request.NewRole;
                await _userRepository.UpdateAsync(user.Id!, user);
                return Ok("Successfully updated");
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return StatusCode(500, "An error occurred while setting the user role: " + ex.Message);
            }
        }

        [HttpPut("user/deactivate")]
        public async Task<IActionResult> DeactivateUser([FromBody] DeactivateRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(request.UserName);
                if (user == null)
                {
                    return NotFound("User doesn't exist");
                }
                if (user.Deactivated == request.Deactivated)
                {
            // Either return a 304 Not Modified or a custom message.
            return StatusCode(304, "No changes detected; user deactivation remains unchanged.");
                }

                user.Deactivated = request.Deactivated;
                await _userRepository.UpdateAsync(user.Id!, user);
                return Ok("Successfully updated");
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return StatusCode(500, "An error occurred while deactivating the user: " + ex.Message);
            }
        }
    }

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
    public async Task<IActionResult> SetUserRole([FromBody] SetUserRoleRequestDTO request)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(request.Token);
        var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

        if(role != Roles.ADMIN.ToString())
        {
            ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
            return Unauthorized(ModelState);
        }

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
    public async Task<IActionResult> DeactivateUser([FromBody] DeactivateRequestDTO request)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(request.Token);
        var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

        if(role != Roles.ADMIN.ToString())
        {
            ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
            return Unauthorized(ModelState);
        }

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
