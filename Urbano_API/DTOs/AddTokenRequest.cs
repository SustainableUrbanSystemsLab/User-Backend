public class AddTokenRequest
{
    public string UserId { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int Quantity { get; set; } = 0;
}

public class RemoveTokenRequest
{
    public string UserId { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int Quantity { get; set; } = 0;
}

public class BalanceRequest
{
    public string UserId { get; set; } = null!;
}

public class VerifyTokenRequest
{
    public string UserId { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int RequiredQuantity { get; set; } = 0;
}

