public class AddTokenRequest
{
    public string Token { get; set; }
    public string UserName { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int Quantity { get; set; } = 0;
}

public class RemoveTokenRequest
{
    public string Token { get; set; }
    public string UserName { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int Quantity { get; set; } = 0;
}

public class BalanceRequest
{
    public string Token { get; set; }
    public string UserName { get; set; } = null!;
}

public class VerifyTokenRequest
{
    public string Token { get; set; }
    public string UserName { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int RequiredQuantity { get; set; } = 0;
}