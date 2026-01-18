using System;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;

namespace MarketplaceSale.Domain.Entities
{
    public sealed class OrderReturnStatus : Entity<Guid>
    {
        private OrderReturnStatus() { }

        public Guid OrderId { get; private set; }
        public Guid SellerId { get; private set; }
        public ReturnStatus Status { get; private set; }

        internal OrderReturnStatus(Guid orderId, Guid sellerId, ReturnStatus status)
            : base(Guid.NewGuid())
        {
            if (orderId == Guid.Empty) throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
            if (sellerId == Guid.Empty) throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));

            OrderId = orderId;
            SellerId = sellerId;
            Status = status;
        }

        internal void ChangeStatus(ReturnStatus status) => Status = status;
    }
}
