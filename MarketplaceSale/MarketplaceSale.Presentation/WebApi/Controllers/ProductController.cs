using MarketplaceSale.WebHost.Requests.Product;
using MarketplaceSale.WebHost.Responses.Product;
using MarketplaceSale.Application.Models.Product;
using MarketplaceSale.Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceSale.WebHost.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductController(IProductApplicationService productService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
            return BadRequest("ProductName is required.");

        if (request.Price <= 0)
            return BadRequest("Price must be greater than 0.");

        if (request.StockQuantity < 0)
            return BadRequest("StockQuantity must be non-negative.");

        var productId = await productService.CreateProductAsync(
            new CreateProductModel(
                request.SellerId,
                request.ProductName,
                request.Description,
                request.Price,
                request.StockQuantity),
            cancellationToken);

        if (productId == Guid.Empty)
            return BadRequest("Product was not created. Seller might not exist.");

        var product = await productService.GetProductByIdAsync(productId, cancellationToken);
        if (product is null)
            return BadRequest("Product was not created.");

        return CreatedAtAction(nameof(GetById), new { id = productId }, ToResponse(product));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={id} not found.");

        return Ok(ToResponse(product));
    }

    [HttpGet("seller/{sellerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductResponse>))]
    public async Task<IActionResult> GetBySellerId(Guid sellerId, CancellationToken cancellationToken)
    {
        var products = await productService.GetProductsBySellerIdAsync(sellerId, cancellationToken);
        var responses = products.Select(ToResponse).ToList();
        return Ok(responses);
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductResponse>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetProductsAsync(cancellationToken);
        var responses = products.Select(ToResponse).ToList();
        return Ok(responses);
    }

    [HttpPut("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePrice(
        Guid id,
        [FromBody] ChangePriceRequest request,
        CancellationToken cancellationToken)
    {
        if (request.NewPrice <= 0)
            return BadRequest("NewPrice must be greater than 0.");

        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={id} not found.");

        await productService.ChangePriceAsync(product.SellerId, id, request.NewPrice, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/stock/increase")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncreaseStock(
        Guid id,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than 0.");

        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={id} not found.");

        await productService.IncreaseStockAsync(product.SellerId, id, request.Quantity, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/stock/decrease")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecreaseStock(
        Guid id,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than 0.");

        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={id} not found.");

        await productService.DecreaseStockAsync(product.SellerId, id, request.Quantity, cancellationToken);
        return NoContent();
    }

    private static ProductResponse ToResponse(ProductModel model) => new()
    {
        Id = model.Id,
        SellerId = model.SellerId,
        ProductName = model.ProductName,
        Description = model.Description,
        Price = model.Price,
        StockQuantity = model.StockQuantity,
        ListingStatus = model.ListingStatus.ToString()
    };

    [HttpPost("{id:guid}/unlist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlist(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={id} not found.");

        await productService.UnlistAsync(product.SellerId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/list")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound($"Product with id={id} not found.");

        await productService.ListAsync(product.SellerId, id, cancellationToken);
        return NoContent();
    }

}
