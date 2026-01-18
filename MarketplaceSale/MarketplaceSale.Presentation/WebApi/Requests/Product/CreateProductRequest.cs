namespace MarketplaceSale.WebHost.Requests.Product;

public sealed class CreateProductRequest
{
    public Guid SellerId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
