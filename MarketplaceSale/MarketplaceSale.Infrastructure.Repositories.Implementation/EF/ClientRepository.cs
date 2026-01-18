using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;
using MarketplaceSale.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF;

public sealed class ClientRepository(ApplicationDbContext context)
    : EfRepository<Client, Guid>(context), IClientRepository
{
    public Task<Client?> GetByUsernameAsync(string username, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Clients.AsNoTracking() : context.Clients;
        var uname = new Username(username);

        return query.FirstOrDefaultAsync(c => c.Username == uname, ct);
    }

    public Task<Client?> GetByIdWithCartAsync(Guid clientId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Clients.AsNoTracking() : context.Clients;
        return query.Include(c => c.Cart).FirstOrDefaultAsync(c => c.Id == clientId, ct);
    }

    public Task<Client?> GetByUsernameWithCartAsync(string username, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Clients.AsNoTracking() : context.Clients;
        var uname = new Username(username);

        return query.Include(c => c.Cart).FirstOrDefaultAsync(c => c.Username == uname, ct);
    }

    public Task<Client?> GetByIdWithCartAndLinesAsync(Guid clientId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = asNoTracking ? context.Clients.AsNoTracking() : context.Clients;

        return query
            .Include(c => c.Cart)
                .ThenInclude(cart => cart.CartLines)
                    .ThenInclude(line => line.Product)
                        .ThenInclude(p => p.Seller)
            // опционально: .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);
    }

}
