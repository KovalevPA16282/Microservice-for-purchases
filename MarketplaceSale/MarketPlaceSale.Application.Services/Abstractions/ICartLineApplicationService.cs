using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Application.Models.CartLine;

namespace MarketplaceSale.Application.Services.Abstractions;

public interface ICartLineApplicationService
{
    Task<CartLineModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CartLineModel>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken);
    Task<CartLineModel?> GetByCartIdAndProductIdAsync(Guid cartId, Guid productId, CancellationToken cancellationToken);
}
