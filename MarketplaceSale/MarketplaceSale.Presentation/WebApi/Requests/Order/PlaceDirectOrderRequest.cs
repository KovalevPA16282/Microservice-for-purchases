namespace MarketplaceSale.WebHost.Requests.Order;

public sealed class PlaceDirectOrderRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
