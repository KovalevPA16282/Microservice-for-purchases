using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface IOrderRepository : IRepository<Order, Guid>
{
    /// <summary>
    /// Для чтения/простых операций: заказ + строки заказа.
    /// </summary>
    Task<Order?> GetByIdWithLinesAsync(
        Guid orderId,
        CancellationToken cancellationToken,
        bool asNoTracking = false
    );

    /// <summary>
    /// Для команд возврата (Request/Approve/Reject/Refund):
    /// OrderLines + return-rows, чтобы ReturnedProducts/ReturnStatuses были корректны.
    /// </summary>
    Task<Order?> GetByIdWithLinesAndReturnsAsync(
        Guid orderId,
        CancellationToken cancellationToken,
        bool asNoTracking = false
    );

    Task<IReadOnlyList<Order>> GetAllByClientIdWithLinesAsync(
        Guid clientId,
        CancellationToken cancellationToken,
        bool asNoTracking = true
    );

    /// <summary>
    /// ✅ Новый метод: получить все заказы клиента со строками и возвратами
    /// </summary>
    Task<IReadOnlyList<Order>> GetAllByClientIdWithLinesAndReturnsAsync(
        Guid clientId,
        CancellationToken cancellationToken,
        bool asNoTracking = true
    );
}
