using System;
using MarketplaceSale.Application.Models.Base;

namespace MarketplaceSale.Application.Models.CartLine;

public sealed record CreateCartLineModel(Guid ProductId, int Quantity) : ICreateModel;
