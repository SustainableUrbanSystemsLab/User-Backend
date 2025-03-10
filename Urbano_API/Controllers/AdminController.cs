using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Urbano_API.Services;
using Urbano_API.Models;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using Urbano_API.DTOs;
using Microsoft.Extensions.Logging;

namespace Urbano_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IUserRepository userRepository, ILogger<AdminController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRateLimit([FromBody] string userName, string token, int maxAttempts)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

                if (role != Roles.ADMIN.ToString())
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

                await _userRepository.UpdateAsync(user.Id!, user);
                return Ok("Successfully updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rate limit");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/role-get/{username}")]
        public async Task<IActionResult> GetUserRole(string username)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(username);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(new { Role = user.Role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user role");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("user/role-set")]
        public async Task<IActionResult> SetUserRole([FromBody] SetUserRoleRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(request.Token);
                var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

                if (role != Roles.ADMIN.ToString())
                {
                    ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
                    return Unauthorized(ModelState);
                }

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
                _logger.LogError(ex, "Error setting user role");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("user/deactivate")]
        public async Task<IActionResult> DeactivateUser([FromBody] DeactivateRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(request.Token);
                var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

                if (role != Roles.ADMIN.ToString())
                {
                    ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
                    return Unauthorized(ModelState);
                }

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
                _logger.LogError(ex, "Error deactivating user");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

