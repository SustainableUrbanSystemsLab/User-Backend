using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Urbano_API.Repositories;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public HomeController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<IActionResult> HomePage([FromBody] string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var userName = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

        var user = await _userRepository.GetUserAsync(userName);

        if (user == null)
        {
            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        if (user.Date.CompareTo(DateTime.Today.AddHours(-24)) <= 0)
        {
            user.AttemptsLeft = user.MaxAttempts;
            user.Date = DateTime.Now;
            await _userRepository.UpdateAsync(user.Id!, user);
        }
        else if (user.AttemptsLeft == 0)
        {
            ModelState.AddModelError("Unauthorized", "Request Limit Reached");
            return Unauthorized(ModelState);
        }

        user.AttemptsLeft -= 1;
        await _userRepository.UpdateAsync(user.Id!, user);

        return Ok("Succesfully logged in");
    }
}