namespace MarketplaceSale.WebHost.Responses.Product;

public sealed class ProductResponse
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string ListingStatus { get; set; } = null!;
}
