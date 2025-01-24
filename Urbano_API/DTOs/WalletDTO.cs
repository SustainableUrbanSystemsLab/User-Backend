using Urbano_API.Models;

namespace Urbano_API.DTOs;

public class WalletDTO
{
   public List<QuotaTokenDTO> QuotaTokens { get; set; } = new List<QuotaTokenDTO>();

    public Wallet GetWallet(string userId)
    {
        Wallet wallet = new Wallet(userId);

        wallet.QuotaTokens = this.QuotaTokens.Select(t => new QuotaToken
        {
            Type = t.Type,
            Quantity = t.Quantity
        }).ToList();

        return wallet;
    }
}

public class QuotaTokenDTO
{
    public string Type { get; set; } = null!;

    public int Quantity { get; set; } = 0;
}