using MarketplaceSale.Application.Models.Order;

namespace MarketplaceSale.Application.Services.Abstractions;

public enum OrderCommandStatus
{
    Ok,
    NotFound,
    Forbidden
}

public interface IOrderApplicationService
{
    Task<OrderModel?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderModel>> GetOrdersByClientIdAsync(Guid clientId, CancellationToken cancellationToken);

    Task<Guid> PlaceSelectedOrderFromCartAsync(Guid clientId, CancellationToken cancellationToken);
    Task<Guid> PlaceDirectOrderAsync(Guid clientId, Guid productId, int quantity, CancellationToken cancellationToken);

    Task<OrderCommandStatus> PayForOrderAsync(Guid clientId, Guid orderId, CancellationToken cancellationToken);
    Task<OrderCommandStatus> CancelOrderAsync(Guid clientId, Guid orderId, CancellationToken cancellationToken);

    Task<OrderCommandStatus> MarkAsShippedAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderCommandStatus> MarkAsDeliveredAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderCommandStatus> MarkAsCompletedAsync(Guid clientId, Guid orderId, CancellationToken cancellationToken);

    Task<OrderCommandStatus> RequestReturnAsync(
        Guid clientId,
        Guid orderId,
        Guid orderLineId,
        int quantity,
        CancellationToken cancellationToken);

    Task<OrderCommandStatus> ApproveReturnAsync(Guid sellerId, Guid orderId, CancellationToken cancellationToken);
    Task<OrderCommandStatus> RejectReturnAsync(Guid sellerId, Guid orderId, CancellationToken cancellationToken);
}
