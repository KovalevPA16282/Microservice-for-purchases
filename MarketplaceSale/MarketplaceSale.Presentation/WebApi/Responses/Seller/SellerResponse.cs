namespace MarketplaceSale.WebHost.Responses.Seller;

public sealed class SellerResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public decimal BusinessBalance { get; set; }
}
