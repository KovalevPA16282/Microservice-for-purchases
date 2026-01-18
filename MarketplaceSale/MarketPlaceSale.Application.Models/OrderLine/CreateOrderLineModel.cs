using System;
using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.OrderLine;

public sealed record CreateOrderLineModel(
    Guid SellerId,
    Guid ProductId,
    int Quantity
) : ICreateModel;
