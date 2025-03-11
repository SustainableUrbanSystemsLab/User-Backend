namespace Urbano_API.DTOs;

public class SetUserRoleRequestDTO
{
    public string? Token { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
}