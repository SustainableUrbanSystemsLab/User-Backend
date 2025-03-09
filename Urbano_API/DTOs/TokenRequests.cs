public class AddTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
}

public class RemoveTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
}

public class BalanceRequest
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class VerifyTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; } = 0;
}

