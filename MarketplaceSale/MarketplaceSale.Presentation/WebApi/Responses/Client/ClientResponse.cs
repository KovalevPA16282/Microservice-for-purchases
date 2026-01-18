namespace MarketplaceSale.WebHost.Responses.Client;

public sealed class ClientResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public decimal AccountBalance { get; set; }
    public Guid CartId { get; set; }
}
