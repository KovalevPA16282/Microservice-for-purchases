using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Application.Models.Product;

namespace MarketplaceSale.Application.Services.Abstractions;

public interface IProductApplicationService
{
    Task<ProductModel?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductModel>> GetProductsAsync(CancellationToken cancellationToken);

    Task<Guid> CreateProductAsync(CreateProductModel productInformation, CancellationToken cancellationToken);
    Task DeleteProductAsync(Guid sellerId, Guid productId, CancellationToken cancellationToken);

    Task ChangePriceAsync(Guid sellerId, Guid productId, decimal newPrice, CancellationToken cancellationToken);
    Task IncreaseStockAsync(Guid sellerId, Guid productId, int quantity, CancellationToken cancellationToken);
    Task DecreaseStockAsync(Guid sellerId, Guid productId, int quantity, CancellationToken cancellationToken);
    Task UnlistAsync(Guid sellerId, Guid productId, CancellationToken cancellationToken);
    Task ListAsync(Guid sellerId, Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductModel>> GetProductsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken); //  получить вообще все товары (0 в наличии, снятые с продажи)
    //Task<IReadOnlyList<ProductModel>> GetProductsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken); // получить все доступные к продаже товары

}

