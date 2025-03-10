namespace Urbano_API.DTOs;

public class AddTokenRequestDTO
{
    public string? Token { get; set; }
    public string UserName { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int Quantity { get; set; } = 0;
}

public class RemoveTokenRequestDTO
{
    public string? Token { get; set; }
    public string UserName { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int Quantity { get; set; } = 0;
}

public class BalanceRequestDTO
{
    public string? Token { get; set; }
    public string UserName { get; set; } = null!;
}

public class VerifyTokenRequestDTO
{
    public string? Token { get; set; }
    public string UserName { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int RequiredQuantity { get; set; } = 0;
}