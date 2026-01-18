using MarketplaceSale.WebHost.Requests.Seller;
using MarketplaceSale.WebHost.Responses.Seller;
using MarketplaceSale.Application.Models.Seller;
using MarketplaceSale.Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceSale.WebHost.Controllers;

[ApiController]
[Route("api/v1/sellers")]
public sealed class SellerController(ISellerApplicationService sellerService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SellerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterSellerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required.");

        var sellerId = await sellerService.RegisterAsync(
            new CreateSellerModel(request.Username),
            cancellationToken);

        var seller = await sellerService.GetByIdAsync(sellerId, cancellationToken);
        if (seller is null)
            return BadRequest("Seller was not created.");

        return CreatedAtAction(nameof(GetById), new { id = sellerId }, ToResponse(seller));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SellerResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var seller = await sellerService.GetByIdAsync(id, cancellationToken);
        if (seller is null)
            return NotFound($"Seller with id={id} not found.");

        return Ok(ToResponse(seller));
    }

    [HttpGet("by-username/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SellerResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken cancellationToken)
    {
        var seller = await sellerService.GetByUsernameAsync(username, cancellationToken);
        if (seller is null)
            return NotFound($"Seller with username={username} not found.");

        return Ok(ToResponse(seller));
    }

    [HttpPut("{id:guid}/username")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeUsername(
        Guid id,
        [FromBody] ChangeSellerUsernameRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewUsername))
            return BadRequest("NewUsername is required.");

        var existing = await sellerService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound($"Seller with id={id} not found.");

        await sellerService.ChangeUsernameAsync(id, request.NewUsername, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(decimal))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusinessBalance(Guid id, CancellationToken cancellationToken)
    {
        var existing = await sellerService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound($"Seller with id={id} not found.");

        var balance = await sellerService.GetBusinessBalanceAsync(id, cancellationToken);
        return Ok(balance);
    }

    private static SellerResponse ToResponse(SellerModel model) => new()
    {
        Id = model.Id,
        Username = model.Username,
        BusinessBalance = model.BusinessBalance
    };
}
