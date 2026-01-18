using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    /// <summary>
    /// Представляет одну строку заказа, содержащую информацию о товаре, его количестве и продавце.
    /// </summary>
    public class OrderLine : Entity<Guid>
    {
        #region Properties

        public Order Order { get; private set; } = null!;
        public Guid OrderId { get; private set; }

        public Product Product { get; private set; } = null!;
        public Quantity Quantity { get; private set; }

        // Главное изменение: фиксируем продавца по Id (не по ссылке).
        public Guid SellerId { get; private set; }

        // Навигационное свойство можно оставить (например, для EF),
        // но в логике сравнивать лучше по SellerId.
        public Seller Seller { get; private set; } = null!;

        #endregion

        #region Constructor

        private OrderLine() { } // EF

        public OrderLine(Product product, Quantity quantity) : base(Guid.NewGuid())
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null)
                throw new ArgumentNullValueException(nameof(quantity));

            if (quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            if (product.Seller is null)
                throw new ProductWithoutSellerException(product);

            Product = product;
            Quantity = quantity;

            Seller = product.Seller;
            SellerId = product.Seller.Id;
        }

        #endregion

        #region Behavior

        internal void AttachToOrder(Order order)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            if (OrderId != Guid.Empty && OrderId != order.Id)
                throw new InvalidOperationException("OrderLine is already attached to another order.");

            Order = order;
            OrderId = order.Id;
        }

        public void IncreaseQuantity(Quantity amount)
        {
            if (amount is null)
                throw new ArgumentNullValueException(nameof(amount));

            if (amount.Value <= 0)
                throw new QuantityMustBePositiveException(Product, amount);

            Quantity = new Quantity(Quantity.Value + amount.Value);
        }

        public void DecreaseQuantity(Quantity amount)
        {
            if (amount is null)
                throw new ArgumentNullValueException(nameof(amount));

            if (amount.Value <= 0)
                throw new QuantityMustBePositiveException(Product, amount);

            if (amount.Value > Quantity.Value)
                throw new QuantityDecreaseExceedsAvailableException(Product, amount, Quantity);

            Quantity = new Quantity(Quantity.Value - amount.Value);
        }

        #endregion
    }
}