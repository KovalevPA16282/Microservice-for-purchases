using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.ValueObjects;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Application.Models.Client;
using MarketplaceSale.Application.Models.Order;
using MarketplaceSale.Application.Models.OrderLine;
using MarketplaceSale.Application.Models.Product;
using MarketplaceSale.Application.Models.Seller;
using MarketplaceSale.Application.Models.Cart;
using MarketplaceSale.Application.Models.CartLine;

namespace MarketplaceSale.Application.Services.Mapping;

public sealed class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        // ValueObjects -> primitives
        CreateMap<Money, decimal>().ConvertUsing(x => x.Value);
        CreateMap<Quantity, int>().ConvertUsing(x => x.Value);
        CreateMap<Username, string>().ConvertUsing(x => x.Value);
        CreateMap<Description, string>().ConvertUsing(x => x.Value);
        CreateMap<ProductName, string>().ConvertUsing(x => x.Value);

        CreateMap<OrderDate, DateTime>().ConvertUsing(x => x.Value);
        // Если DeliveryDate тоже ValueObject<DateTime>:
        CreateMap<DeliveryDate, DateTime>().ConvertUsing(x => x.Value);

        // Dictionary<Guid, Quantity> -> Dictionary<Guid, int>
        CreateMap<IReadOnlyDictionary<Guid, Quantity>, IReadOnlyDictionary<Guid, int>>()
            .ConvertUsing(src => src.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value));

        // Entities -> Models
        CreateMap<Product, ProductModel>()
            .ForCtorParam(nameof(ProductModel.SellerId),
                opt => opt.MapFrom(src => src.Seller.Id))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductName.Value))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description.Value))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Value))
            .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.StockQuantity.Value))
            .ForMember(d => d.ListingStatus, o => o.MapFrom(s => s.ListingStatus));


        CreateMap<OrderLine, OrderLineModel>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Product.Id))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => s.SellerId))
            .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity.Value));


        CreateMap<Order, OrderModel>()
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Value))
            .ForMember(d => d.OrderDate, o => o.MapFrom(s => s.OrderDate.Value))
            .ForMember(d => d.DeliveryDate, o => o.MapFrom(s => s.DeliveryDate != null ? s.DeliveryDate.Value : (DateTime?)null))
            .ForMember(d => d.OrderLines, o => o.MapFrom(s => s.OrderLines))
            .ForMember(d => d.ReturnedProducts, o => o.MapFrom(s =>
                s.ReturnedProducts.Select(kvp => new OrderModel.ReturnedProductModel(
                    kvp.Key.SellerId,
                    kvp.Key.ProductId,
                    kvp.Value.Value
                )).ToList()
            ))
            .ForMember(d => d.ReturnStatuses, o => o.MapFrom(s => s.ReturnStatuses));


        CreateMap<Client, ClientModel>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.Username.Value))
            .ForMember(d => d.AccountBalance, o => o.MapFrom(s => s.AccountBalance.Value))
            .ForMember(d => d.CartId, o => o.MapFrom(s => s.Cart.Id))
            .ForMember(d => d.PurchaseHistory, o => o.MapFrom(s => s.PurchaseHistory));
                // .ForMember(d => d.ReturnHistory, o => o.MapFrom(s => s.ReturnHistory)); // удалить


        CreateMap<Seller, SellerModel>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.Username.Value))
            .ForMember(d => d.BusinessBalance, o => o.MapFrom(s => s.BusinessBalance.Value))
            .ForMember(d => d.AvailableProducts, o => o.MapFrom(s => s.AvailableProducts));

        CreateMap<CartLine, CartLineModel>();

        CreateMap<Cart, CartModel>()
            .ForCtorParam(nameof(CartModel.ClientId), opt => opt.MapFrom(src => src.Client.Id))
            .ForMember(d => d.CartLines, opt => opt.MapFrom(src => src.CartLines));

    }
}
