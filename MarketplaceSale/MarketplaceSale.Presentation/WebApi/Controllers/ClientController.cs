using Microsoft.AspNetCore.Mvc;
using MarketplaceSale.Application.Models.Client;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.WebHost.Requests.Client;
using MarketplaceSale.WebHost.Responses.Client;

namespace MarketplaceSale.WebHost.Controllers;

[ApiController]
[Route("api/v1/clients")]
public sealed class ClientController(IClientApplicationService clientService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClientResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterClientRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required.");

        var clientId = await clientService.RegisterAsync(
            new CreateClientModel(request.Username),
            cancellationToken);

        var client = await clientService.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return BadRequest("Client was not created.");

        return CreatedAtAction(nameof(GetById), new { id = clientId }, ToResponse(client));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(id, cancellationToken);
        if (client is null)
            return NotFound($"Client with id={id} not found.");

        return Ok(ToResponse(client));
    }

    [HttpGet("by-username/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken cancellationToken)
    {
        var client = await clientService.GetByUsernameAsync(username, cancellationToken);
        if (client is null)
            return NotFound($"Client with username={username} not found.");

        return Ok(ToResponse(client));
    }

    [HttpPut("{id:guid}/username")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeUsername(
        Guid id,
        [FromBody] ChangeUsernameRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewUsername))
            return BadRequest("NewUsername is required.");

        var result = await clientService.ChangeUsernameAsync(id, request.NewUsername, cancellationToken);

        return result switch
        {
            ClientCommandResult.Ok => NoContent(),
            ClientCommandResult.NoChanges => NoContent(), // можно и 200 OK, но 204 ок
            ClientCommandResult.NotFound => NotFound($"Client with id={id} not found."),
            ClientCommandResult.Invalid => BadRequest("Invalid username."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("{id:guid}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(decimal))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(Guid id, CancellationToken cancellationToken)
    {
        var existing = await clientService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound($"Client with id={id} not found.");

        var balance = await clientService.GetBalanceAsync(id, cancellationToken);
        return Ok(balance);
    }

    [HttpPost("{id:guid}/balance/top-up")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TopUpBalance(
        Guid id,
        [FromBody] TopUpBalanceRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        var result = await clientService.TopUpBalanceAsync(id, request.Amount, cancellationToken);

        return result switch
        {
            ClientCommandResult.Ok => NoContent(),
            ClientCommandResult.NotFound => NotFound($"Client with id={id} not found."),
            ClientCommandResult.Invalid => BadRequest("Invalid top up amount."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private static ClientResponse ToResponse(ClientModel model) => new()
    {
        Id = model.Id,
        Username = model.Username,
        AccountBalance = model.AccountBalance,
        CartId = model.CartId
    };
}
