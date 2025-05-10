namespace Urbano_API.DTOs;

using Urbano_API.Models;

public class LoginDTO
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public CommunityType Community { get; set; }
}