using AutoMapper;
using MarketplaceSale.Application.Models.Cart;
using MarketplaceSale.Application.Models.CartLine;
using MarketplaceSale.Application.Models.Client;
using MarketplaceSale.Application.Models.Order;
using MarketplaceSale.Application.Models.OrderLine;
using MarketplaceSale.Application.Models.Product;
using MarketplaceSale.Application.Models.Seller;
using MarketplaceSale.WebHost.Requests.Cart;
using MarketplaceSale.WebHost.Requests.Client;
using MarketplaceSale.WebHost.Requests.Order;
using MarketplaceSale.WebHost.Requests.Product;
using MarketplaceSale.WebHost.Requests.Seller;
using MarketplaceSale.WebHost.Responses.Cart;
using MarketplaceSale.WebHost.Responses.Client;
using MarketplaceSale.WebHost.Responses.Order;
using MarketplaceSale.WebHost.Responses.Product;
using MarketplaceSale.WebHost.Responses.Seller;

namespace MarketplaceSale.WebHost.Mapping
{
    public class PresentationProfile : Profile
    {
        public PresentationProfile()
        {
            // ========== CLIENT ==========
            CreateMap<ClientModel, ClientResponse>();
            CreateMap<RegisterClientRequest, CreateClientModel>();
            CreateMap<ChangeUsernameRequest, ClientModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.NewUsername));
            CreateMap<TopUpBalanceRequest, ClientModel>();

            // ========== SELLER ==========
            CreateMap<SellerModel, SellerResponse>();
            CreateMap<RegisterSellerRequest, CreateSellerModel>();
            CreateMap<ChangeSellerUsernameRequest, SellerModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.NewUsername));

            // ========== PRODUCT ==========
            CreateMap<ProductModel, ProductResponse>();
            CreateMap<CreateProductRequest, CreateProductModel>();
            CreateMap<ChangePriceRequest, ProductModel>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.NewPrice));
            CreateMap<AdjustStockRequest, ProductModel>()
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Quantity));

            // ========== CART ==========
            CreateMap<CartModel, CartResponse>();
            CreateMap<AddToCartRequest, CreateCartLineModel>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
            CreateMap<ChangeCartLineQuantityRequest, CartLineModel>()
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.NewQuantity));
            CreateMap<SelectProductRequest, CartLineModel>();

            // ========== CART LINE ==========
            CreateMap<CartLineModel, CartResponse>();

            // ========== ORDER ==========
            CreateMap<OrderModel, OrderResponse>();
            CreateMap<PlaceOrderFromCartRequest, CreateOrderModel>();
            CreateMap<PlaceDirectOrderRequest, CreateOrderModel>()
                .ForMember(dest => dest.ClientId, opt => opt.Ignore());
            CreateMap<PayForOrderRequest, OrderModel>();
            CreateMap<MarkAsShippedRequest, OrderModel>();
            CreateMap<MarkAsDeliveredRequest, OrderModel>();
            CreateMap<RequestReturnRequest, OrderModel>();

            // ========== ORDER LINE ==========
            CreateMap<OrderLineModel, OrderResponse>();
        }
    }
}
