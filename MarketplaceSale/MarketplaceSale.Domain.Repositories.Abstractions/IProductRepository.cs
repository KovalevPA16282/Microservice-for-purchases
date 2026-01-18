using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetByIdWithSellerAsync(
        Guid id,
        CancellationToken ct,
        bool asNoTracking = true);

    Task<IReadOnlyList<Product>> GetAllWithSellerAsync(
        CancellationToken ct,
        bool asNoTracking = true);

    Task<IReadOnlyList<Product>> GetAllBySellerIdWithSellerAsync(
    Guid sellerId,
    CancellationToken ct,
    bool asNoTracking = true);

}
