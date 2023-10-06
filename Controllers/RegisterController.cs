using System;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Urbano_API.Models;
using Urbano_API.Services;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    private readonly AuthService _authService;

    public RegisterController(AuthService authService) =>
        _authService = authService;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if(!isValidUserName(user.UserName))
        {
            return BadRequest("Incorrect mail Id");
        }
        var resp = await _authService.GetUserAsync(user.UserName);

        if(resp is null)
        {
            await _authService.CreateAsync(user);

            return Ok("User Succesfully created");
        }
        return BadRequest("User already exists");
    }

    private bool isValidUserName(string userName)
    {
        var valid = true;

        try
        {
            var emailAddress = new MailAddress(userName);
        }
        catch
        {
            valid = false;
        }

        return valid;
    }
}

