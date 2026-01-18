using MarketplaceSale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Domain.Exceptions
{
    public class OrderDoesNotBelongToSellerException(Order order, Seller seller)
        : InvalidOperationException($"Order with ID '{order.Id}' does not belong to seller '{seller.Username}'.")
    {
        public Order Order => order;
        public Seller Seller => seller;
    }
}
