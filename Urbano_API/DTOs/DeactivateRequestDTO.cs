namespace Urbano_API.DTOs;

public class DeactivateRequestDTO
{
    public string? Token { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool Deactivated { get; set; } = false;
}