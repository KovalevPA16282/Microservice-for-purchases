using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class OrderRepository(ApplicationDbContext context)
    : EfRepository<Order, Guid>(context), IOrderRepository
{
    public Task<Order?> GetByIdWithLinesAsync(Guid orderId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Orders.AsNoTracking() : context.Orders;

        return query
            .Include(o => o.Client)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Seller)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllByClientIdWithLinesAsync(
        Guid clientId,
        CancellationToken ct,
        bool asNoTracking = true)
    {
        var query = asNoTracking ? context.Orders.AsNoTracking() : context.Orders;

        var orders = await query
            .Include(o => o.Client)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Seller)
            .AsSplitQuery()
            .Where(o => o.Client.Id == clientId)
            .ToListAsync(ct);

        return orders;
    }

    public Task<Order?> GetByIdWithLinesAndReturnsAsync(Guid orderId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Orders.AsNoTracking() : context.Orders;

        return query
            .Include(o => o.Client)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Seller)
            .Include("_returnedProductsRows")
            .Include("_returnStatusesRows")
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllByClientIdWithLinesAndReturnsAsync(
        Guid clientId,
        CancellationToken ct,
        bool asNoTracking = true)
    {
        var query = asNoTracking ? context.Orders.AsNoTracking() : context.Orders;

        var orders = await query
            .Include(o => o.Client)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines).ThenInclude(ol => ol.Seller)
            .Include("_returnedProductsRows")
            .Include("_returnStatusesRows")
            .AsSplitQuery()
            .Where(o => o.Client.Id == clientId)
            .ToListAsync(ct);

        return orders;
    }
}
