using System;
using System.Collections.Generic;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Application.Models.Base;
using MarketplaceSale.Application.Models.OrderLine;

namespace MarketplaceSale.Application.Models.Order;

public sealed record OrderModel(Guid Id, Guid ClientId) : IModel<Guid>
{
    public sealed record ReturnedProductModel(
        Guid SellerId,
        Guid ProductId,
        int Quantity
    );

    public required IReadOnlyList<OrderLineModel> OrderLines { get; init; }

    public required IReadOnlyList<ReturnedProductModel> ReturnedProducts { get; init; }

    public required IReadOnlyDictionary<Guid, ReturnStatus> ReturnStatuses { get; init; }

    public required decimal TotalAmount { get; init; }
    public required OrderStatus Status { get; init; }

    public required DateTime OrderDate { get; init; }
    public DateTime? DeliveryDate { get; init; }
}
