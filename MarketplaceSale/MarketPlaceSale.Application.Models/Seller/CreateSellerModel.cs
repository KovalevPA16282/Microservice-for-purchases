using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.Seller;

public sealed record CreateSellerModel(string Username) : ICreateModel;
