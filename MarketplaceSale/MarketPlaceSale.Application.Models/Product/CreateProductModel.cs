using System;
using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.Product;

public sealed record CreateProductModel(
    Guid SellerId,
    string ProductName,
    string Description,
    decimal Price,
    int StockQuantity
) : ICreateModel;
