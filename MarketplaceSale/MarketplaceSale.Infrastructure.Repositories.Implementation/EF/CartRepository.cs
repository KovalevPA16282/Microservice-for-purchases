using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class CartRepository(ApplicationDbContext context)
    : EfRepository<Cart, Guid>(context), ICartRepository
{
    public Task<Cart?> GetByClientIdAsync(Guid clientId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Carts.AsNoTracking() : context.Carts;

        return query
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Client.Id == clientId, ct);
    }

    public Task<Cart?> GetByClientIdWithLinesAsync(Guid clientId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Carts.AsNoTracking() : context.Carts;

        return query
            .Include(c => c.Client)
            .Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Client.Id == clientId, ct);
    }
}
