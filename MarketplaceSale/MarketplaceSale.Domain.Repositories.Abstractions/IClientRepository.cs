using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Repositories.Abstractions;

public interface IClientRepository : IRepository<Client, Guid>
{
    Task<Client?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken,
        bool asNoTracking = false);

    // Для команд: агрегат целиком, tracked по умолчанию
    Task<Client?> GetByIdWithCartAsync(
        Guid clientId,
        CancellationToken cancellationToken,
        bool asNoTracking = false);

    // ✅ Для чтения/клиентских DTO: клиент + корзина по username (nullable)
    Task<Client?> GetByUsernameWithCartAsync(
        string username,
        CancellationToken cancellationToken,
        bool asNoTracking = false);

    Task<Client?> GetByIdWithCartAndLinesAsync(
        Guid clientId, 
        CancellationToken ct, 
        bool asNoTracking = false);
}
