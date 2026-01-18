namespace MarketplaceSale.WebHost.Requests.Seller;

public sealed class ChangeSellerUsernameRequest
{
    public string NewUsername { get; set; } = null!;
}
