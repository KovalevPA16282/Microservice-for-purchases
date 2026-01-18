using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class OrderLineRepository(ApplicationDbContext context)
    : EfRepository<OrderLine, Guid>(context), IOrderLineRepository
{
    public Task<OrderLine?> GetByOrderIdAndProductIdAsync(Guid orderId, Guid productId, CancellationToken ct, bool asNoTracking = true)
    {
        var query = asNoTracking ? context.OrderLines.AsNoTracking() : context.OrderLines;

        return query
            .Include(ol => ol.Order)
            .Include(ol => ol.Product)
            .FirstOrDefaultAsync(ol => ol.OrderId == orderId && ol.Product.Id == productId, ct);
    }

    public async Task<IReadOnlyList<OrderLine>> GetAllByOrderIdAsync(Guid orderId, CancellationToken ct, bool asNoTracking = true)
    {
        var query = asNoTracking ? context.OrderLines.AsNoTracking() : context.OrderLines;

        var orderLines = await query
            .Include(ol => ol.Order)
            .Include(ol => ol.Product)
            .Where(ol => ol.OrderId == orderId)
            .ToListAsync(ct);

        return orderLines;
    }

    public Task<bool> ExistsAsync(Guid orderId, Guid productId, CancellationToken ct)
    {
        var query = context.OrderLines.AsNoTracking();
        return query
            .AnyAsync(ol => ol.OrderId == orderId && ol.Product.Id == productId, ct);
    }
}
