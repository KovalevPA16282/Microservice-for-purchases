using System;


using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.Cart;

public sealed record CreateCartModel(Guid ClientId) : ICreateModel;
