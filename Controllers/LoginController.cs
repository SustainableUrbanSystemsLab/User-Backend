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

    public LoginController(IConfiguration configuration, AuthService authService)
    {
        this.configuration = configuration;
        _authService = authService;
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
        if (resp.UserName == credential.UserName && credential.Password == resp.Password)
        {
            // Creating the security context
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, credential.UserName),
                };


            return Ok(new
            {
                access_token = CreateToken(claims),
            });

        }

        ModelState.AddModelError("Unauthorized", "You are not authorized to access the endpoint.");
        return Unauthorized(ModelState);
    }

    private string CreateToken(IEnumerable<Claim> claims)
    {
        var secretKey = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey") ?? "");

        // generate the JWT
        var jwt = new JwtSecurityToken(
                claims: claims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }


    public class Credential
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

