using AutoMapper;
using MarketplaceSale.Application.Models.Order;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarketplaceSale.Application.Services;

public sealed class OrderApplicationService(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IOrderApplicationService
{
    public async Task<OrderModel?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdWithLinesAndReturnsAsync(
            orderId,
            cancellationToken,
            asNoTracking: true);

        return order is null ? null : mapper.Map<OrderModel>(order);
    }

    public async Task<IReadOnlyList<OrderModel>> GetOrdersByClientIdAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Orders.GetAllByClientIdWithLinesAndReturnsAsync(
            clientId,
            cancellationToken,
            asNoTracking: true);

        return orders.Select(mapper.Map<OrderModel>).ToList();
    }

    public async Task<Guid> PlaceSelectedOrderFromCartAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var client = await unitOfWork.Clients.GetByIdWithCartAndLinesAsync(
                clientId,
                cancellationToken,
                asNoTracking: false);

            if (client is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Guid.Empty;
            }

            var order = client.PlaceSelectedOrderFromCart();

            await unitOfWork.Orders.AddAsync(order, cancellationToken);
            await unitOfWork.Clients.UpdateAsync(client, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return order.Id;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Guid> PlaceDirectOrderAsync(
        Guid clientId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var client = await unitOfWork.Clients.GetByIdAsync(
                clientId,
                cancellationToken,
                asNoTracking: false);

            if (client is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Guid.Empty;
            }

            var product = await unitOfWork.Products.GetByIdWithSellerAsync(
                productId,
                cancellationToken,
                asNoTracking: false);

            if (product is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Guid.Empty;
            }

            var order = client.PlaceDirectOrder(product, new Quantity(quantity));

            await unitOfWork.Orders.AddAsync(order, cancellationToken);
            await unitOfWork.Clients.UpdateAsync(client, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return order.Id;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderCommandStatus> PayForOrderAsync(
        Guid clientId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var client = await unitOfWork.Clients.GetByIdAsync(
                clientId,
                cancellationToken,
                asNoTracking: false);

            if (client is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            var order = await unitOfWork.Orders.GetByIdWithLinesAsync(
                orderId,
                cancellationToken,
                asNoTracking: false);

            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            if (order.Client.Id != clientId)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.Forbidden;
            }

            client.PayForOrder(order);

            await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.Clients.UpdateAsync(client, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return OrderCommandStatus.Ok;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderCommandStatus> CancelOrderAsync(
        Guid clientId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var client = await unitOfWork.Clients.GetByIdAsync(
                clientId,
                cancellationToken,
                asNoTracking: false);

            if (client is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            var order = await unitOfWork.Orders.GetByIdWithLinesAsync(
                orderId,
                cancellationToken,
                asNoTracking: false);

            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            if (order.Client.Id != clientId)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.Forbidden;
            }

            client.CancelOrder(order);

            await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.Clients.UpdateAsync(client, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return OrderCommandStatus.Ok;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderCommandStatus> MarkAsShippedAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdWithLinesAsync(
            orderId,
            cancellationToken,
            asNoTracking: false);

        if (order is null)
            return OrderCommandStatus.NotFound;

        order.MarkAsShipped();

        await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderCommandStatus.Ok;
    }

    public async Task<OrderCommandStatus> MarkAsDeliveredAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdWithLinesAsync(
            orderId,
            cancellationToken,
            asNoTracking: false);

        if (order is null)
            return OrderCommandStatus.NotFound;

        order.MarkAsDelivered();

        await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderCommandStatus.Ok;
    }

    public async Task<OrderCommandStatus> MarkAsCompletedAsync(
        Guid clientId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdWithLinesAsync(
            orderId,
            cancellationToken,
            asNoTracking: false);

        if (order is null)
            return OrderCommandStatus.NotFound;

        if (order.Client.Id != clientId)
            return OrderCommandStatus.Forbidden;

        order.MarkAsCompleted();

        await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderCommandStatus.Ok;
    }

    public async Task<OrderCommandStatus> RequestReturnAsync(
    Guid clientId,
    Guid orderId,
    Guid orderLineId,
    int quantity,
    CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var client = await unitOfWork.Clients.GetByIdAsync(
                clientId,
                cancellationToken,
                asNoTracking: false);

            if (client is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            var order = await unitOfWork.Orders.GetByIdWithLinesAndReturnsAsync(
                orderId,
                cancellationToken,
                asNoTracking: false);

            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            if (order.Client.Id != clientId)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.Forbidden;
            }

            var orderLine = order.OrderLines.FirstOrDefault(l => l.Id == orderLineId);
            if (orderLine is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            client.RequestProductReturn(order, orderLine.Seller, orderLine.Product, new Quantity(quantity));

            await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.Clients.UpdateAsync(client, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return OrderCommandStatus.Ok;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }


    public async Task<OrderCommandStatus> ApproveReturnAsync(
        Guid sellerId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var seller = await unitOfWork.Sellers.GetByIdAsync(
                sellerId,
                cancellationToken,
                asNoTracking: false);

            if (seller is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            var order = await unitOfWork.Orders.GetByIdWithLinesAndReturnsAsync(
                orderId,
                cancellationToken,
                asNoTracking: false);

            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            if (!order.OrderLines.Any(l => l.SellerId == sellerId))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.Forbidden;
            }

            seller.ApproveOrderReturn(order);

            await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.Sellers.UpdateAsync(seller, cancellationToken);
            await unitOfWork.Clients.UpdateAsync(order.Client, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return OrderCommandStatus.Ok;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderCommandStatus> RejectReturnAsync(
        Guid sellerId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var seller = await unitOfWork.Sellers.GetByIdAsync(
                sellerId,
                cancellationToken,
                asNoTracking: false);

            if (seller is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            var order = await unitOfWork.Orders.GetByIdWithLinesAndReturnsAsync(
                orderId,
                cancellationToken,
                asNoTracking: false);

            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.NotFound;
            }

            if (!order.OrderLines.Any(l => l.SellerId == sellerId))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return OrderCommandStatus.Forbidden;
            }

            seller.RejectOrderReturn(order);

            await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.Sellers.UpdateAsync(seller, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return OrderCommandStatus.Ok;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
