using System;
using System.Collections.Generic;
using MarketplaceSale.Application.Models.Base;
using MarketplaceSale.Application.Models.Product;

namespace MarketplaceSale.Application.Models.Seller;

public sealed record SellerModel(Guid Id, string Username, decimal BusinessBalance) : IModel<Guid>
{
    public required IReadOnlyList<ProductModel> AvailableProducts { get; init; }
}
