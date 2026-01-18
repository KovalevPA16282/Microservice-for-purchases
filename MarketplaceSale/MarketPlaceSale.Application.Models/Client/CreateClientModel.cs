using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.Client;

public sealed record CreateClientModel(string Username) : ICreateModel;
