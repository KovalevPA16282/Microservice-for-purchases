using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Application.Models.Client;

namespace MarketplaceSale.Application.Services.Abstractions;

public enum ClientCommandResult
{
    Ok,
    NotFound,
    Invalid,
    NoChanges
}

public interface IClientApplicationService
{
    Task<Guid> RegisterAsync(CreateClientModel clientInformation, CancellationToken cancellationToken);

    Task<ClientModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ClientModel?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<ClientModel?> GetByUsernameWithCartAsync(string username, CancellationToken cancellationToken);

    Task<ClientCommandResult> ChangeUsernameAsync(Guid clientId, string newUsername, CancellationToken cancellationToken);

    Task<decimal> GetBalanceAsync(Guid clientId, CancellationToken cancellationToken);
    Task<ClientCommandResult> TopUpBalanceAsync(Guid clientId, decimal amount, CancellationToken cancellationToken);
}
