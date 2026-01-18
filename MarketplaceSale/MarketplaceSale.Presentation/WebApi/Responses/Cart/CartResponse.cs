namespace MarketplaceSale.WebHost.Responses.Cart;

public sealed class CartResponse
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public List<CartLineResponse> CartLines { get; set; } = new();
}

public sealed class CartLineResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string SelectionStatus { get; set; } = null!;
}
