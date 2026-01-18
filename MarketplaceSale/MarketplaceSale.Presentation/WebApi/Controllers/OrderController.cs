using MarketplaceSale.Application.Models.Order;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.WebHost.Requests.Order;
using MarketplaceSale.WebHost.Responses.Order;
using Microsoft.AspNetCore.Mvc;
using MarketplaceSale.Domain.Exceptions;

namespace MarketplaceSale.WebHost.Controllers;

[ApiController]
[Route("api/v1/orders")]
public sealed class OrderController(
    IOrderApplicationService orderService,
    IClientApplicationService clientService,
    ISellerApplicationService sellerService,
    IProductApplicationService productService
) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        return Ok(ToResponse(order));
    }

    [HttpGet("client/{clientId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OrderResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClientId(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        var orders = await orderService.GetOrdersByClientIdAsync(clientId, cancellationToken);
        var responses = orders.Select(ToResponse).ToList();
        return Ok(responses);
    }

    [HttpPost("from-cart")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PlaceFromCart(
        [FromQuery] Guid clientId,
        CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        Guid orderId;
        try
        {
            orderId = await orderService.PlaceSelectedOrderFromCartAsync(clientId, cancellationToken);
        }
        catch (CartSelectionEmptyException)
        {
            return BadRequest("No selected products in cart. Select products before placing an order.");
        }

        if (orderId == Guid.Empty)
            return BadRequest("Order was not created. Cart might be empty or not enough funds/stock.");

        var order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
            return BadRequest("Order was not created.");

        return CreatedAtAction(nameof(GetById), new { id = orderId }, ToResponse(order));
    }

    [HttpPost("direct")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PlaceDirect(
    [FromQuery] Guid clientId,
    [FromBody] PlaceDirectOrderRequest request,
    CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than 0.");

        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        var product = await productService.GetProductByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={request.ProductId} not found.");

        Guid orderId;
        try
        {
            orderId = await orderService.PlaceDirectOrderAsync(
                clientId,
                request.ProductId,
                request.Quantity,
                cancellationToken);
        }
        catch (NotEnoughStockException)
        {
            return BadRequest("Order was not created. Not enough stock.");
        }
        catch (QuantityMustBePositiveException)
        {
            return BadRequest("Quantity must be greater than 0.");
        }
        catch (ProductWithoutSellerException)
        {
            return BadRequest("Order was not created. Product is not available for sale.");
        }

        if (orderId == Guid.Empty)
            return BadRequest("Order was not created.");

        var order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
            return BadRequest("Order was not created.");

        return CreatedAtAction(nameof(GetById), new { id = orderId }, ToResponse(order));
    }


    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(
        [FromQuery] Guid clientId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (order.ClientId != clientId)
            return Forbid();

        if (!string.Equals(order.Status.ToString(), "Pending", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order must be in Pending status to pay.");

        var balance = await clientService.GetBalanceAsync(clientId, cancellationToken);
        if (balance < order.TotalAmount)
            return BadRequest("Not enough funds to pay for this order.");

        var status = await orderService.PayForOrderAsync(clientId, id, cancellationToken);

        return status switch
        {
            OrderCommandStatus.Ok => NoContent(),
            OrderCommandStatus.NotFound => NotFound("Client or order not found."),
            OrderCommandStatus.Forbidden => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromQuery] Guid clientId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (order.ClientId != clientId)
            return Forbid();

        if (!string.Equals(order.Status.ToString(), "Paid", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order must be in Paid status to cancel.");

        var status = await orderService.CancelOrderAsync(clientId, id, cancellationToken);

        return status switch
        {
            OrderCommandStatus.Ok => NoContent(),
            OrderCommandStatus.NotFound => NotFound("Client or order not found."),
            OrderCommandStatus.Forbidden => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("{id:guid}/mark-shipped")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsShipped(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (!string.Equals(order.Status.ToString(), "Paid", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order must be in Paid status to mark shipped.");

        var status = await orderService.MarkAsShippedAsync(id, cancellationToken);

        return status switch
        {
            OrderCommandStatus.Ok => NoContent(),
            OrderCommandStatus.NotFound => NotFound("Order not found."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("{id:guid}/mark-delivered")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsDelivered(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (!string.Equals(order.Status.ToString(), "Shipped", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order must be in Shipped status to mark delivered.");

        var status = await orderService.MarkAsDeliveredAsync(id, cancellationToken);

        return status switch
        {
            OrderCommandStatus.Ok => NoContent(),
            OrderCommandStatus.NotFound => NotFound("Order not found."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("{id:guid}/mark-completed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsCompleted(
        [FromQuery] Guid clientId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (order.ClientId != clientId)
            return Forbid();

        if (!string.Equals(order.Status.ToString(), "Delivered", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order must be in Delivered status to mark completed.");

        var status = await orderService.MarkAsCompletedAsync(clientId, id, cancellationToken);

        return status switch
        {
            OrderCommandStatus.Ok => NoContent(),
            OrderCommandStatus.NotFound => NotFound("Client or order not found."),
            OrderCommandStatus.Forbidden => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("{id:guid}/request-return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestReturn(
        [FromQuery] Guid clientId,
        Guid id,
        [FromBody] RequestReturnRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than 0.");

        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={clientId} not found.");

        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (order.ClientId != clientId)
            return Forbid();

        if (!string.Equals(order.Status.ToString(), "Completed", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order must be in Completed status to request return.");

        var line = order.OrderLines.FirstOrDefault(l => l.Id == request.OrderLineId);
        if (line is null)
            return BadRequest("Order line is not present in the order.");

        if (request.Quantity > line.Quantity)
            return BadRequest("Return quantity exceeds ordered quantity.");

        try
        {
            var status = await orderService.RequestReturnAsync(
                clientId, id, request.OrderLineId, request.Quantity, cancellationToken);

            return status switch
            {
                OrderCommandStatus.Ok => NoContent(),
                OrderCommandStatus.NotFound => NotFound("Client or order not found."),
                OrderCommandStatus.Forbidden => Forbid(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
        catch (ReturnAlreadyInProgressException)
        {
            return BadRequest("Return for this seller is already in progress (already requested/approved/rejected).");
        }
    }

    [HttpPost("{id:guid}/approve-return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveReturn(
    [FromQuery] Guid sellerId,
    Guid id,
    CancellationToken cancellationToken)
    {
        var seller = await sellerService.GetByIdAsync(sellerId, cancellationToken);
        if (seller is null)
            return NotFound($"Seller with id={sellerId} not found.");

        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (!order.OrderLines.Any(l => l.SellerId == sellerId))
            return Forbid();

        try
        {
            var status = await orderService.ApproveReturnAsync(sellerId, id, cancellationToken);

            return status switch
            {
                OrderCommandStatus.Ok => NoContent(),
                OrderCommandStatus.NotFound => NotFound("Seller or order not found."),
                OrderCommandStatus.Forbidden => Forbid(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
        catch (ReturnNotRequestedException)
        {
            return BadRequest("Return is not in Requested status (already approved/rejected or not requested).");
        }
        catch (OrderDoesNotBelongToSellerException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("No return items found for this seller", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("No return request exists for this seller in this order.");
        }
    }


    [HttpPost("{id:guid}/reject-return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectReturn(
    [FromQuery] Guid sellerId,
    Guid id,
    CancellationToken cancellationToken)
    {
        var seller = await sellerService.GetByIdAsync(sellerId, cancellationToken);
        if (seller is null)
            return NotFound($"Seller with id={sellerId} not found.");

        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound($"Order with id={id} not found.");

        if (!order.OrderLines.Any(l => l.SellerId == sellerId))
            return Forbid();

        try
        {
            var status = await orderService.RejectReturnAsync(sellerId, id, cancellationToken);

            return status switch
            {
                OrderCommandStatus.Ok => NoContent(),
                OrderCommandStatus.NotFound => NotFound("Seller or order not found."),
                OrderCommandStatus.Forbidden => Forbid(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
        catch (ReturnNotRequestedException)
        {
            return BadRequest("Return is not in Requested status (already approved/rejected or not requested).");
        }
        catch (OrderDoesNotBelongToSellerException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("No return items found for this seller", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("No return request exists for this seller in this order.");
        }
    }


    private static OrderResponse ToResponse(OrderModel model) => new()
    {
        Id = model.Id,
        ClientId = model.ClientId,
        TotalAmount = model.TotalAmount,
        Status = model.Status.ToString(),
        OrderDate = model.OrderDate,
        DeliveryDate = model.DeliveryDate,

        OrderLines = model.OrderLines
            .Select(ol => new OrderLineResponse
            {
                Id = ol.Id,
                ProductId = ol.ProductId,
                SellerId = ol.SellerId,
                Quantity = ol.Quantity
            })
            .ToList(),

        ReturnedProducts = model.ReturnedProducts
            .Select(rp => new ReturnedProductResponse
            {
                SellerId = rp.SellerId,
                ProductId = rp.ProductId,
                Quantity = rp.Quantity
            })
            .ToList(),

        ReturnStatuses = model.ReturnStatuses
            .Select(kvp => new ReturnStatusResponse
            {
                SellerId = kvp.Key,
                Status = kvp.Value.ToString()
            })
            .ToList()
    };
}
