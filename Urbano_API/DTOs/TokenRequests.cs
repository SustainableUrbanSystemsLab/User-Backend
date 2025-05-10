namespace Urbano_API.DTOs;

public class AddTokenRequestDTO
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
}

public class RemoveTokenRequestDTO
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
}

public class BalanceRequestDTO
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class VerifyTokenRequestDTO
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; } = 0;
}