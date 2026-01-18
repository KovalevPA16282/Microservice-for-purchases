using System;

namespace MarketplaceSale.Application.Models.Base;

public interface IModel<TId>
    where TId : struct, IEquatable<TId>
{
    TId Id { get; }
}
