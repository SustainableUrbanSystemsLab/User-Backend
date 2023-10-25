using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using Urbano_API.Models;
using Urbano_API.Services;

namespace Urbano_API.Controllers;

[ApiController]
[Route("[controller]")]
public class GenerateOTPController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly VerificationService _verificationService;

    public GenerateOTPController(AuthService authService, VerificationService verificationService)
    {
        _authService = authService;
        _verificationService = verificationService;
    }

    [HttpPost]
    public async Task<IActionResult> validateEmail([FromBody] Email emailObj)
    {
        if (!isValidUserName(emailObj.UserName))
        {
            return BadRequest("Incorrect mail Id");
        }
        var resp = await _authService.GetUserAsync(emailObj.UserName);
        if (resp is null)
        {
            return BadRequest("Incorrect mail Id");
        }

        _verificationService.sendOTP(resp.UserName, resp.FirstName);

        return Ok("Password change request sent to mail");
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

    public class Email
    {
        public string UserName { get; set; } = string.Empty;
    }
}

