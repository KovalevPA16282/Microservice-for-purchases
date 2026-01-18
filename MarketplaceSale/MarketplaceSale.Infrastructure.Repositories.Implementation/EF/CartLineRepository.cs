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

public sealed class CartLineRepository(ApplicationDbContext context)
    : EfRepository<CartLine, Guid>(context), ICartLineRepository
{
    public Task<CartLine?> GetByCartIdAndProductIdAsync(Guid cartId, Guid productId, CancellationToken ct, bool asNoTracking = true)
    {
        var query = asNoTracking ? context.CartLines.AsNoTracking() : context.CartLines;

        return query
            .Include(cl => cl.Cart)
            .Include(cl => cl.Product)
            .FirstOrDefaultAsync(cl => cl.Cart.Id == cartId && cl.Product.Id == productId, ct);
    }

    public async Task<IReadOnlyList<CartLine>> GetAllByCartIdAsync(Guid cartId, CancellationToken ct, bool asNoTracking = true)
    {
        var query = asNoTracking ? context.CartLines.AsNoTracking() : context.CartLines;

        var cartLines = await query
            .Include(cl => cl.Cart)
            .Include(cl => cl.Product)
            .Where(cl => cl.Cart.Id == cartId)
            .ToListAsync(ct);

        return cartLines;  // List<T> -> IReadOnlyList<T>
    }

    public Task<bool> ExistsAsync(Guid cartId, Guid productId, CancellationToken ct)
    {
        var query = context.CartLines.AsNoTracking();
        return query
            .AnyAsync(cl => cl.Cart.Id == cartId && cl.ProductId == productId, ct);
    }
}
