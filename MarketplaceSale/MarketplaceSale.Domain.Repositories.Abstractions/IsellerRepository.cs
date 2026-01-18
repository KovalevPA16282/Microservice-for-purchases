using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface ISellerRepository : IRepository<Seller, Guid>
{
    Task<Seller?> GetByUsernameAsync(string username, CancellationToken cancellationToken, bool asNoTracking = false);

    Task<Seller?> GetByIdWithProductsAsync(Guid sellerId, CancellationToken cancellationToken, bool asNoTracking = false);

    Task<Seller?> GetByUsernameWithProductsAsync(string username, CancellationToken cancellationToken, bool asNoTracking = false);
}
