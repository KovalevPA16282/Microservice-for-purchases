namespace MarketplaceSale.WebHost.Requests.Order;

public sealed class RequestReturnRequest
{
    public Guid OrderLineId { get; set; }
    public int Quantity { get; set; }
}
