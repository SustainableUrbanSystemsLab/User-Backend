namespace Urbano_API.DTOs;

public class EmailChangeDTO
{
    public string Token { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
}

