using System;
using System.Collections.Generic;
using MarketplaceSale.Application.Models.Base;
using MarketplaceSale.Application.Models.Order;

namespace MarketplaceSale.Application.Models.Client
{
    public sealed record class ClientModel(
        Guid Id,
        string Username,
        decimal AccountBalance,
        Guid CartId
    ) : IModel<Guid>
    {
        public required IReadOnlyCollection<OrderModel> PurchaseHistory { get; init; }
    }
}
