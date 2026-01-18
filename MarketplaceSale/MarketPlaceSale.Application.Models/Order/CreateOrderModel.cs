using System;
using System.Collections.Generic;
using MarketplaceSale.Application.Models.Base;
using MarketplaceSale.Application.Models.OrderLine;

namespace MarketplaceSale.Application.Models.Order;

public sealed record CreateOrderModel(
    Guid ClientId,
    IReadOnlyList<CreateOrderLineModel> Lines
) : ICreateModel;
