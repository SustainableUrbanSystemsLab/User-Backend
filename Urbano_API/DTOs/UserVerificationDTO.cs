namespace Urbano_API.DTOs;

public class UserVerificationDTO
{
    public string OTP { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}