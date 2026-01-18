using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MarketplaceSale.Application.Models.Product;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Application.Services;

public sealed class ProductApplicationService(
    IProductRepository productRepository,
    ISellerRepository sellerRepository,
    IMapper mapper
) : IProductApplicationService
{
    public async Task<ProductModel?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdWithSellerAsync(
            productId,
            cancellationToken,
            asNoTracking: true);

        return product is null ? null : mapper.Map<ProductModel>(product);
    }

    public async Task<IReadOnlyList<ProductModel>> GetProductsAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllWithSellerAsync(
            cancellationToken,
            asNoTracking: true);

        return products.Select(mapper.Map<ProductModel>).ToList();
    }

    public async Task<Guid> CreateProductAsync(CreateProductModel productInformation, CancellationToken cancellationToken)
    {
        var seller = await sellerRepository.GetByIdAsync(
            productInformation.SellerId,
            cancellationToken,
            asNoTracking: false);

        if (seller is null) return Guid.Empty;

        var product = new Product(
            new ProductName(productInformation.ProductName),
            new Description(productInformation.Description),
            new Money(productInformation.Price),
            new Quantity(productInformation.StockQuantity),
            seller
        );

        await productRepository.AddAsync(product, cancellationToken);
        return product.Id;
    }

    public async Task DeleteProductAsync(Guid sellerId, Guid productId, CancellationToken cancellationToken)
    {
        var seller = await sellerRepository.GetByIdWithProductsAsync(
            sellerId,
            cancellationToken,
            asNoTracking: false);

        if (seller is null) return;

        var product = await productRepository.GetByIdAsync(
            productId,
            cancellationToken,
            asNoTracking: false);

        if (product is null) return;

        seller.DeleteProduct(product);

        await productRepository.DeleteAsync(productId, cancellationToken);
        await sellerRepository.UpdateAsync(seller, cancellationToken);
    }

    public async Task ChangePriceAsync(Guid sellerId, Guid productId, decimal newPrice, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null) return;

        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: false);
        if (seller is null) return;

        product.ChangePrice(new Money(newPrice), seller);
        await productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task IncreaseStockAsync(Guid sellerId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null) return;

        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: false);
        if (seller is null) return;

        product.SellerIncreaseStock(seller, new Quantity(quantity));
        await productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task DecreaseStockAsync(Guid sellerId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null) return;

        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: false);
        if (seller is null) return;

        product.SellerDecreaseStock(seller, new Quantity(quantity));
        await productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task UnlistAsync(Guid sellerId, Guid productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null) return;

        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: false);
        if (seller is null) return;

        product.RemoveFromSale(seller);
        await productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task ListAsync(Guid sellerId, Guid productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken, asNoTracking: false);
        if (product is null) return;

        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: false);
        if (seller is null) return;

        product.ReturnToSale(seller);
        await productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductModel>> GetProductsBySellerIdAsync(
    Guid sellerId,
    CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllBySellerIdWithSellerAsync(
            sellerId,
            cancellationToken,
            asNoTracking: true);

        return products.Select(mapper.Map<ProductModel>).ToList();
    }

}
