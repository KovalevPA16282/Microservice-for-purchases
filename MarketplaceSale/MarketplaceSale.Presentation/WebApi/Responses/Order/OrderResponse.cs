namespace MarketplaceSale.WebHost.Responses.Order;

public sealed class OrderResponse
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public List<OrderLineResponse> OrderLines { get; set; } = new();
    public List<ReturnedProductResponse> ReturnedProducts { get; set; } = new();

    // NEW: статусы возврата по sellerId
    public List<ReturnStatusResponse> ReturnStatuses { get; set; } = new();
}

public sealed class OrderLineResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public int Quantity { get; set; }
}

public sealed class ReturnedProductResponse
{
    public Guid SellerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class ReturnStatusResponse
{
    public Guid SellerId { get; set; }
    public string Status { get; set; } = null!;
}
