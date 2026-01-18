using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Repositories.Abstractions;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    IOrderRepository Orders { get; }
    IClientRepository Clients { get; }
    ISellerRepository Sellers { get; }
    IProductRepository Products { get; }
    ICartRepository Carts { get; }
    ICartLineRepository CartLines { get; }
    IOrderLineRepository OrderLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
