using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Application.Models.Cart;

namespace MarketplaceSale.Application.Services.Abstractions;

public enum CartCommandResult
{
    Ok,
    NotFound,
    Invalid
}

public interface ICartApplicationService
{
    Task<CartModel?> GetCartByClientIdAsync(Guid clientId, CancellationToken cancellationToken);

    Task<CartCommandResult> AddToCartAsync(Guid clientId, Guid productId, int quantity, CancellationToken cancellationToken);
    Task<CartCommandResult> ChangeQuantityAsync(Guid clientId, Guid productId, int newQuantity, CancellationToken cancellationToken);

    Task<CartCommandResult> RemoveFromCartAsync(Guid clientId, Guid productId, CancellationToken cancellationToken);
    Task<CartCommandResult> ClearCartAsync(Guid clientId, CancellationToken cancellationToken);

    Task<CartCommandResult> SelectProductAsync(Guid clientId, Guid productId, CancellationToken cancellationToken);
    Task<CartCommandResult> UnselectProductAsync(Guid clientId, Guid productId, CancellationToken cancellationToken);
}
