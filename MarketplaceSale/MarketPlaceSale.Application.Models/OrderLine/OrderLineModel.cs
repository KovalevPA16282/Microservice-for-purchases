using System;
using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.OrderLine;

public sealed record OrderLineModel(
    Guid Id,
    Guid ProductId,
    Guid SellerId,
    int Quantity
) : IModel<Guid>;
