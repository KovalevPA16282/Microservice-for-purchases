using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;
using System;

namespace MarketplaceSale.Domain.Entities
{
    public class CartLine : Entity<Guid>
    {
        #region Properties

        public Guid ProductId { get; private set; }

        public Cart Cart { get; private set; } = null!;
        public Product Product { get; private set; } = null!;
        public Quantity Quantity { get; private set; } = null!;
        public CartSelectionStatus SelectionStatus { get; private set; }

        #endregion

        #region Constructor

        private CartLine() { }

        internal CartLine(Cart cart, Product product, Quantity quantity) : base(Guid.NewGuid())
        {
            Cart = cart ?? throw new ArgumentNullValueException(nameof(cart));
            Product = product ?? throw new ArgumentNullValueException(nameof(product));
            ProductId = product.Id;

            Quantity = quantity ?? throw new ArgumentNullValueException(nameof(quantity));
            if (Quantity.Value <= 0)
                throw new QuantityMustBePositiveException(Product, Quantity);

            SelectionStatus = CartSelectionStatus.Unselected;
        }

        #endregion

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

        public void SelectProduct() => SelectionStatus = CartSelectionStatus.Selected;
        public void UnselectProduct() => SelectionStatus = CartSelectionStatus.Unselected;

        public Money GetPrice() => Product.Price * Quantity.Value;
    }
}
