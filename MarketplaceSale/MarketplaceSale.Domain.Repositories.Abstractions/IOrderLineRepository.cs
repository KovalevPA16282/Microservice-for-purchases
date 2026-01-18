using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface IOrderLineRepository : IReadRepository<OrderLine, Guid>
{
    Task<OrderLine?> GetByOrderIdAndProductIdAsync(
        Guid orderId,
        Guid productId,
        CancellationToken cancellationToken,
        bool asNoTracking = true);

    Task<IReadOnlyList<OrderLine>> GetAllByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken,
        bool asNoTracking = true);

    Task<bool> ExistsAsync(
        Guid orderId,
        Guid productId,
        CancellationToken cancellationToken);
}
