using System;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Application.Models.Seller;

namespace MarketplaceSale.Application.Services.Abstractions;

public interface ISellerApplicationService
{
    Task<Guid> RegisterAsync(CreateSellerModel sellerInformation, CancellationToken cancellationToken);

    Task<SellerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SellerModel?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    Task ChangeUsernameAsync(Guid sellerId, string newUsername, CancellationToken cancellationToken);

    Task<decimal> GetBusinessBalanceAsync(Guid sellerId, CancellationToken cancellationToken);
}
