using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface ICartLineRepository : IReadRepository<CartLine, Guid>
{
    Task<CartLine?> GetByCartIdAndProductIdAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken,
        bool asNoTracking = true);

    Task<IReadOnlyList<CartLine>> GetAllByCartIdAsync(
        Guid cartId,
        CancellationToken cancellationToken,
        bool asNoTracking = true);

    Task<bool> ExistsAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken);
}
