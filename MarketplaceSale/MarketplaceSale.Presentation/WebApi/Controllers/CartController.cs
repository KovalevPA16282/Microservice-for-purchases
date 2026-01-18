using MarketplaceSale.WebHost.Requests.Cart;
using MarketplaceSale.WebHost.Responses.Cart;
using MarketplaceSale.Application.Models.Cart;
using MarketplaceSale.Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceSale.WebHost.Controllers;

[ApiController]
[Route("api/v1/carts")]
public sealed class CartController(ICartApplicationService cartService) : ControllerBase
{
    [HttpGet("client/{clientId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClientId(Guid clientId, CancellationToken cancellationToken)
    {
        var cart = await cartService.GetCartByClientIdAsync(clientId, cancellationToken);
        if (cart is null)
            return NotFound($"Cart for client with id={clientId} not found.");

        return Ok(ToResponse(cart));
    }

    [HttpPost("client/{clientId:guid}/products")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToCart(
        Guid clientId,
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than 0.");

        var result = await cartService.AddToCartAsync(clientId, request.ProductId, request.Quantity, cancellationToken);

        return result switch
        {
            CartCommandResult.Ok => NoContent(),
            CartCommandResult.NotFound => NotFound($"Client id={clientId} or product id={request.ProductId} not found."),
            CartCommandResult.Invalid => BadRequest("Cannot add product to cart."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("client/{clientId:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(
        Guid clientId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await cartService.RemoveFromCartAsync(clientId, productId, cancellationToken);

        return result switch
        {
            CartCommandResult.Ok => NoContent(),
            CartCommandResult.NotFound => NotFound($"Client id={clientId} or product id={productId} not found."),
            CartCommandResult.Invalid => BadRequest("Cannot remove product from cart (maybe it is not in cart)."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPut("client/{clientId:guid}/products/{productId:guid}/quantity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeQuantity(
        Guid clientId,
        Guid productId,
        [FromBody] ChangeCartLineQuantityRequest request,
        CancellationToken cancellationToken)
    {
        if (request.NewQuantity < 0)
            return BadRequest("NewQuantity must be non-negative. Use 0 to remove from cart.");

        var result = await cartService.ChangeQuantityAsync(clientId, productId, request.NewQuantity, cancellationToken);

        return result switch
        {
            CartCommandResult.Ok => NoContent(),
            CartCommandResult.NotFound => NotFound($"Client id={clientId} or cart line for product id={productId} not found."),
            CartCommandResult.Invalid => BadRequest("Cannot change quantity."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("client/{clientId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCart(Guid clientId, CancellationToken cancellationToken)
    {
        var result = await cartService.ClearCartAsync(clientId, cancellationToken);

        return result switch
        {
            CartCommandResult.Ok => NoContent(),
            CartCommandResult.NotFound => NotFound($"Client id={clientId} not found."),
            CartCommandResult.Invalid => BadRequest("Cannot clear cart."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("client/{clientId:guid}/select")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SelectProduct(
        Guid clientId,
        [FromBody] SelectProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cartService.SelectProductAsync(clientId, request.ProductId, cancellationToken);

        return result switch
        {
            CartCommandResult.Ok => NoContent(),
            CartCommandResult.NotFound => NotFound($"Client id={clientId} or product id={request.ProductId} not found."),
            CartCommandResult.Invalid => BadRequest("Cannot select product for order (maybe it is not in cart)."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("client/{clientId:guid}/unselect")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnselectProduct(
        Guid clientId,
        [FromBody] SelectProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cartService.UnselectProductAsync(clientId, request.ProductId, cancellationToken);

        return result switch
        {
            CartCommandResult.Ok => NoContent(),
            CartCommandResult.NotFound => NotFound($"Client id={clientId} or product id={request.ProductId} not found."),
            CartCommandResult.Invalid => BadRequest("Cannot unselect product for order (maybe it is not in cart)."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private static CartResponse ToResponse(CartModel model) => new()
    {
        Id = model.Id,
        ClientId = model.ClientId,
        CartLines = model.CartLines
            .Select(cl => new CartLineResponse
            {
                Id = cl.Id,
                ProductId = cl.ProductId,
                Quantity = cl.Quantity,
                SelectionStatus = cl.SelectionStatus.ToString()
            })
            .ToList()
    };
}
