using System;
using System.Collections.Generic;
using MarketplaceSale.Application.Models.Base;
using MarketplaceSale.Application.Models.CartLine;

namespace MarketplaceSale.Application.Models.Cart;

public sealed record CartModel(Guid Id, Guid ClientId) : IModel<Guid>
{
    public required IReadOnlyList<CartLineModel> CartLines { get; init; }
}
