namespace Urbano_API.DTOs;

public class DeactivateRequestDTO
{
    public string? Token { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool Deactivated { get; set; } = false;
}