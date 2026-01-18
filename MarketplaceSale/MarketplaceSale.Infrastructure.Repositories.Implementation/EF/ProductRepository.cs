using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class ProductRepository(ApplicationDbContext context)
    : EfRepository<Product, Guid>(context), IProductRepository
{
    public Task<Product?> GetByIdWithSellerAsync(Guid id, CancellationToken ct, bool asNoTracking = true)
    {
        var query = asNoTracking ? context.Products.AsNoTracking() : context.Products;
        return query.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllWithSellerAsync(CancellationToken ct, bool asNoTracking = true)
    {
        var query = asNoTracking ? context.Products.AsNoTracking() : context.Products;
        return await query.Include(p => p.Seller).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllBySellerIdWithSellerAsync(
    Guid sellerId,
    CancellationToken ct,
    bool asNoTracking = true)
    {
        var query = asNoTracking ? context.Products.AsNoTracking() : context.Products;

        return await query
            .Include(p => p.Seller)
            .Where(p => p.Seller.Id == sellerId)
            .ToListAsync(ct);
    }

}
