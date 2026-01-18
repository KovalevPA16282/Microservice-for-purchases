using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;
using MarketplaceSale.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class SellerRepository(ApplicationDbContext context)
    : EfRepository<Seller, Guid>(context), ISellerRepository
{
    public Task<Seller?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken,
        bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Sellers.AsNoTracking() : context.Sellers;
        var uname = new Username(username);

        return query.FirstOrDefaultAsync(s => s.Username == uname, cancellationToken);
    }

    public Task<Seller?> GetByIdWithProductsAsync(Guid sellerId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Sellers.AsNoTracking() : context.Sellers;

        return query
            .Include("_products")
            .FirstOrDefaultAsync(s => s.Id == sellerId, ct);
    }

    public Task<Seller?> GetByUsernameWithProductsAsync(string username, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Sellers.AsNoTracking() : context.Sellers;
        var uname = new Username(username);

        return query
            .Include("_products")
            .FirstOrDefaultAsync(s => s.Username == uname, ct);
    }
}
