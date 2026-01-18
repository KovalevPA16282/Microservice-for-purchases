using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MarketplaceSale.Application.Models.Client;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Application.Services;

public sealed class ClientApplicationService(
    IClientRepository clientRepository,
    IMapper mapper
) : IClientApplicationService
{
    public async Task<Guid> RegisterAsync(CreateClientModel clientInformation, CancellationToken cancellationToken)
    {
        var client = new Client(new Username(clientInformation.Username));
        await clientRepository.AddAsync(client, cancellationToken);
        return client.Id;
    }

    public async Task<ClientModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAsync(id, cancellationToken, asNoTracking: true);
        return client is null ? null : mapper.Map<ClientModel>(client);
    }

    public async Task<ClientModel?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByUsernameWithCartAsync(username, cancellationToken, asNoTracking: true);
        return client is null ? null : mapper.Map<ClientModel>(client);
    }

    public async Task<ClientModel?> GetByUsernameWithCartAsync(string username, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByUsernameWithCartAsync(username, cancellationToken, asNoTracking: true);
        return client is null ? null : mapper.Map<ClientModel>(client);
    }

    public async Task<ClientCommandResult> ChangeUsernameAsync(
        Guid clientId,
        string newUsername,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(clientId, cancellationToken, asNoTracking: false);
        if (client is null) return ClientCommandResult.NotFound;

        bool changed;
        try
        {
            changed = client.ChangeUsername(new Username(newUsername));
        }
        catch
        {
            return ClientCommandResult.Invalid;
        }

        if (!changed)
            return ClientCommandResult.NoChanges;

        await clientRepository.UpdateAsync(client, cancellationToken);
        return ClientCommandResult.Ok;
    }

    public async Task<decimal> GetBalanceAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(clientId, cancellationToken, asNoTracking: true);
        return client?.AccountBalance.Value ?? 0m;
    }

    public async Task<ClientCommandResult> TopUpBalanceAsync(
        Guid clientId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(clientId, cancellationToken, asNoTracking: false);
        if (client is null) return ClientCommandResult.NotFound;

        try
        {
            client.AddBalance(new Money(amount));
        }
        catch
        {
            return ClientCommandResult.Invalid;
        }

        await clientRepository.UpdateAsync(client, cancellationToken);
        return ClientCommandResult.Ok;
    }
}
