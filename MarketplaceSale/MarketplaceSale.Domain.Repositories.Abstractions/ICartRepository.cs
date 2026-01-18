using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface ICartRepository : IRepository<Cart, Guid>
{
    /// <summary>
    /// Базовая загрузка корзины по клиенту.
    /// </summary>
    Task<Cart?> GetByClientIdAsync(
        Guid clientId,
        CancellationToken cancellationToken,
        bool asNoTracking = false);

    /// <summary>
    /// Для команд: корзина + строки корзины (CartLines).
    /// </summary>
    Task<Cart?> GetByClientIdWithLinesAsync(
        Guid clientId,
        CancellationToken cancellationToken,
        bool asNoTracking = false);
}
