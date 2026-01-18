using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Application.Models.OrderLine;

namespace MarketplaceSale.Application.Services.Abstractions;

public interface IOrderLineApplicationService
{
    Task<OrderLineModel?> GetOrderLineByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderLineModel>> GetOrderLinesByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderLineModel>> GetOrderLinesByOrderIdAndSellerIdAsync(
        Guid orderId,
        Guid sellerId,
        CancellationToken cancellationToken);
}
