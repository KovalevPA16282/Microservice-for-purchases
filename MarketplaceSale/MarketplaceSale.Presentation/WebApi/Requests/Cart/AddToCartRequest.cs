namespace MarketplaceSale.WebHost.Requests.Cart;

public sealed class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
