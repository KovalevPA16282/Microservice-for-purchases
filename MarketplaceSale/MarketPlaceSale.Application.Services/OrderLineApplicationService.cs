using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MarketplaceSale.Application.Models.OrderLine;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Repositories.Abstractions;

namespace MarketplaceSale.Application.Services;

public sealed class OrderLineApplicationService(
    IOrderLineRepository orderLineRepository,
    IMapper mapper
) : IOrderLineApplicationService
{
    public async Task<OrderLineModel?> GetOrderLineByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await orderLineRepository.GetByIdAsync(id, cancellationToken, asNoTracking: true);
        return entity is null ? null : mapper.Map<OrderLineModel>(entity);
    }

    public async Task<IReadOnlyList<OrderLineModel>> GetOrderLinesByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var entities = await orderLineRepository.GetAllByOrderIdAsync(orderId, cancellationToken, asNoTracking: true);
        return entities.Select(mapper.Map<OrderLineModel>).ToList();
    }

    public async Task<IReadOnlyList<OrderLineModel>> GetOrderLinesByOrderIdAndSellerIdAsync(
        Guid orderId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        var entities = await orderLineRepository.GetAllByOrderIdAsync(orderId, cancellationToken, asNoTracking: true);
        return entities
            .Where(x => x.SellerId == sellerId)
            .Select(mapper.Map<OrderLineModel>)
            .ToList();
    }
}
