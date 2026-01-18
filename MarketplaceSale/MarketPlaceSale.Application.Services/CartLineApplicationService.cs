using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MarketplaceSale.Application.Models.CartLine;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Repositories.Abstractions;

namespace MarketplaceSale.Application.Services;

public sealed class CartLineApplicationService(
    ICartLineRepository cartLineRepository,
    IMapper mapper
) : ICartLineApplicationService
{
    public async Task<CartLineModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await cartLineRepository.GetByIdAsync(id, cancellationToken, asNoTracking: true);
        return entity is null ? null : mapper.Map<CartLineModel>(entity);
    }

    public async Task<IReadOnlyList<CartLineModel>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken)
    {
        var entities = await cartLineRepository.GetAllByCartIdAsync(cartId, cancellationToken, asNoTracking: true);
        return entities.Select(mapper.Map<CartLineModel>).ToList();
    }

    public async Task<CartLineModel?> GetByCartIdAndProductIdAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var entity = await cartLineRepository.GetByCartIdAndProductIdAsync(
            cartId,
            productId,
            cancellationToken,
            asNoTracking: true);

        return entity is null ? null : mapper.Map<CartLineModel>(entity);
    }
}
