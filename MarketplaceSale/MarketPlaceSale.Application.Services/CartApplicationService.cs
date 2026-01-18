using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MarketplaceSale.Application.Models.Cart;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Application.Services;

public sealed class CartApplicationService(
    IClientRepository clientRepository,
    IProductRepository productRepository,
    IMapper mapper
) : ICartApplicationService
{
    public async Task<CartModel?> GetCartByClientIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: true);

        return client is null ? null : mapper.Map<CartModel>(client.Cart);
    }

    public async Task<CartCommandResult> AddToCartAsync(
        Guid clientId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: false);

        if (client is null)
            return CartCommandResult.NotFound;

        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null)
            return CartCommandResult.NotFound;

        try
        {
            client.AddToCart(product, new Quantity(quantity));
            await clientRepository.UpdateAsync(client, cancellationToken);
            return CartCommandResult.Ok;
        }
        catch
        {
            return CartCommandResult.Invalid;
        }
    }

    public async Task<CartCommandResult> RemoveFromCartAsync(
        Guid clientId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: false);

        if (client is null)
            return CartCommandResult.NotFound;

        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null)
            return CartCommandResult.NotFound;

        try
        {
            client.RemoveFromCart(product);
            await clientRepository.UpdateAsync(client, cancellationToken);
            return CartCommandResult.Ok;
        }
        catch
        {
            // например: продукт не в корзине / доменная валидация
            return CartCommandResult.Invalid;
        }
    }

    public async Task<CartCommandResult> ClearCartAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: false);

        if (client is null)
            return CartCommandResult.NotFound;

        client.ClearCart();
        await clientRepository.UpdateAsync(client, cancellationToken);
        return CartCommandResult.Ok;
    }

    public async Task<CartCommandResult> SelectProductAsync(
        Guid clientId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: false);

        if (client is null)
            return CartCommandResult.NotFound;

        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null)
            return CartCommandResult.NotFound;

        try
        {
            client.SelectProductForOrder(product);
            await clientRepository.UpdateAsync(client, cancellationToken);
            return CartCommandResult.Ok;
        }
        catch
        {
            // например: продукта нет в корзине
            return CartCommandResult.Invalid;
        }
    }

    public async Task<CartCommandResult> UnselectProductAsync(
        Guid clientId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: false);

        if (client is null)
            return CartCommandResult.NotFound;

        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null)
            return CartCommandResult.NotFound;

        try
        {
            client.UnselectProductForOrder(product);
            await clientRepository.UpdateAsync(client, cancellationToken);
            return CartCommandResult.Ok;
        }
        catch
        {
            return CartCommandResult.Invalid;
        }
    }

    public async Task<CartCommandResult> ChangeQuantityAsync(
        Guid clientId,
        Guid productId,
        int newQuantity,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithCartAndLinesAsync(
            clientId,
            cancellationToken,
            asNoTracking: false);

        if (client is null)
            return CartCommandResult.NotFound;

        var line = client.Cart.CartLines.FirstOrDefault(x => x.ProductId == productId);
        if (line is null)
            return CartCommandResult.NotFound;

        try
        {
            if (newQuantity <= 0)
            {
                // симметрично RemoveFromCartAsync: достаём продукт отдельно
                var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
                if (product is null)
                    return CartCommandResult.NotFound;

                client.RemoveFromCart(product);
                await clientRepository.UpdateAsync(client, cancellationToken);
                return CartCommandResult.Ok;
            }

            var current = line.Quantity.Value;
            var delta = newQuantity - current;

            if (delta > 0)
                line.IncreaseQuantity(new Quantity(delta));
            else if (delta < 0)
                line.DecreaseQuantity(new Quantity(-delta));

            await clientRepository.UpdateAsync(client, cancellationToken);
            return CartCommandResult.Ok;
        }
        catch
        {
            return CartCommandResult.Invalid;
        }
    }
}
