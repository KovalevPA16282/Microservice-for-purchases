using System;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    public sealed class OrderReturnProduct : Entity<Guid>
    {
        private OrderReturnProduct() { }

        public Guid OrderId { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid ProductId { get; private set; }
        public Quantity Quantity { get; private set; } = null!;

        internal OrderReturnProduct(Guid orderId, Guid sellerId, Guid productId, Quantity quantity)
            : base(Guid.NewGuid())
        {
            if (orderId == Guid.Empty) throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
            if (sellerId == Guid.Empty) throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));
            if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
            if (quantity is null) throw new ArgumentNullValueException(nameof(quantity));
            if (quantity.Value <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));

            OrderId = orderId;
            SellerId = sellerId;
            ProductId = productId;
            Quantity = quantity;
        }

        internal void ChangeQuantity(Quantity quantity)
        {
            if (quantity is null) throw new ArgumentNullValueException(nameof(quantity));
            if (quantity.Value <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));

            Quantity = quantity;
        }
    }
}
