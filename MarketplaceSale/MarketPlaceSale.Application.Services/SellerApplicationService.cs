using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MarketplaceSale.Application.Models.Seller;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Application.Services;

public sealed class SellerApplicationService(
    ISellerRepository sellerRepository,
    IMapper mapper
) : ISellerApplicationService
{
    public async Task<Guid> RegisterAsync(CreateSellerModel sellerInformation, CancellationToken cancellationToken)
    {
        var seller = new Seller(Guid.NewGuid(), new Username(sellerInformation.Username));

        await sellerRepository.AddAsync(seller, cancellationToken);
        return seller.Id;
    }

    public async Task<SellerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // ✅ Для SellerModel нужны продукты, поэтому берём метод с Include продуктов
        var seller = await sellerRepository.GetByIdWithProductsAsync(id, cancellationToken, asNoTracking: true);
        return seller is null ? null : mapper.Map<SellerModel>(seller);
    }

    public async Task<SellerModel?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        // ✅ Один запрос, сразу с продуктами
        var seller = await sellerRepository.GetByUsernameWithProductsAsync(username, cancellationToken, asNoTracking: true);
        return seller is null ? null : mapper.Map<SellerModel>(seller);
    }

    public async Task ChangeUsernameAsync(Guid sellerId, string newUsername, CancellationToken cancellationToken)
    {
        // ✅ Для изменения нужен tracking
        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: false);
        if (seller is null) return;

        seller.ChangeUsername(new Username(newUsername));
        await sellerRepository.UpdateAsync(seller, cancellationToken);
    }

    public async Task<decimal> GetBusinessBalanceAsync(Guid sellerId, CancellationToken cancellationToken)
    {
        // ✅ Баланс не требует Include продуктов
        var seller = await sellerRepository.GetByIdAsync(sellerId, cancellationToken, asNoTracking: true);
        return seller?.BusinessBalance.Value ?? 0m;
    }
}
