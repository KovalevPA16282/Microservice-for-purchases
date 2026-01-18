using System;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.CartLine;

public sealed record CartLineModel(
    Guid Id,
    Guid ProductId,
    int Quantity,
    CartSelectionStatus SelectionStatus
) : IModel<Guid>;
