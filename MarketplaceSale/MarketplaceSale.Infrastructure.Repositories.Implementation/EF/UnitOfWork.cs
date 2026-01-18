using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IOrderRepository Orders { get; }
    public IClientRepository Clients { get; }
    public ISellerRepository Sellers { get; }
    public IProductRepository Products { get; }
    public ICartRepository Carts { get; }
    public ICartLineRepository CartLines { get; }
    public IOrderLineRepository OrderLines { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IOrderRepository orders,
        IClientRepository clients,
        ISellerRepository sellers,
        IProductRepository products,
        ICartRepository carts,
        ICartLineRepository cartLines,
        IOrderLineRepository orderLines)
    {
        _context = context;
        Orders = orders;
        Clients = clients;
        Sellers = sellers;
        Products = products;
        Carts = carts;
        CartLines = cartLines;
        OrderLines = orderLines;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
            throw new InvalidOperationException("Transaction has not been started.");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
            throw new InvalidOperationException("Transaction has not been started.");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }

        await _context.DisposeAsync();
    }
}
