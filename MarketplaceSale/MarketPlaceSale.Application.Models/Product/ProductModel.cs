using System;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.Product;

public sealed record ProductModel(
    Guid Id,
    Guid SellerId,
    string ProductName,
    string Description,
    decimal Price,
    int StockQuantity,
    ProductListingStatus ListingStatus
) : IModel<Guid>;
