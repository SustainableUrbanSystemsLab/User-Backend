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
public class LoginController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly AuthService _authService;
    private readonly VerificationService _verificationService;

    public LoginController(IConfiguration configuration, AuthService authService, VerificationService verificationService)
    {
        this.configuration = configuration;
        _authService = authService;
        _verificationService = verificationService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] Credential credential)
    {

        var resp = await _authService.GetUserAsync(credential.UserName);

        if(resp is null)
        {

            ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        // Verify the credential
        if (resp.Verified == true && resp.UserName == credential.UserName && credential.Password == resp.Password)
        {
            // Creating the security context
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, credential.UserName),
                    new Claim(ClaimTypes.Role, resp.Role),
                };


            return Ok(new
            {
                access_token = _verificationService.CreateToken(claims),
            });

        }

        ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
        return Unauthorized(ModelState);
    }

    public class Credential
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

