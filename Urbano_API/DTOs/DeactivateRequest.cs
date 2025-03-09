public class DeactivateRequest
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool Deactivated { get; set; }
}

