using System;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    public class Product : Entity<Guid>
    {
        #region Properties

        public ProductName ProductName { get; private set; }
        public Description Description { get; private set; }
        public Money Price { get; private set; }
        public Quantity StockQuantity { get; private set; }
        public ProductListingStatus ListingStatus { get; private set; } = ProductListingStatus.Listed;
        public Seller Seller { get; private set; }

        #endregion

        #region Constructors

        protected Product() { }

        public Product(
            ProductName productName,
            Description description,
            Money price,
            Quantity stockQuantity,
            Seller seller)
            : this(Guid.NewGuid(), productName, description, price, stockQuantity, seller) { }

        protected Product(
            Guid id,
            ProductName productName,
            Description description,
            Money price,
            Quantity stockQuantity,
            Seller seller)
            : base(id)
        {
            ProductName = productName ?? throw new ArgumentNullValueException(nameof(productName));
            Description = description ?? throw new ArgumentNullValueException(nameof(description));
            Price = price ?? throw new ArgumentNullValueException(nameof(price));
            StockQuantity = stockQuantity ?? throw new ArgumentNullValueException(nameof(stockQuantity));
            Seller = seller ?? throw new ArgumentNullValueException(nameof(seller));
        }

        #endregion

        #region Behavior

        public void AssignToSeller(Seller seller)
        {
            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            if (Seller != null && Seller.Id != seller.Id)
                throw new ProductAlreadyAssignedToAnotherSellerException(this, Seller, seller);

            Seller = seller;
        }

        private void EnsureSeller(Seller seller)
        {
            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            if (Seller is null || Seller.Id != seller.Id)
                throw new ProductDoesNotBelongToSellerException(this, seller);
        }

        // Новое: проверка по sellerId (без зависимости от инстанса Seller)
        private void EnsureSellerId(Guid sellerId)
        {
            if (sellerId == Guid.Empty)
                throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));

            if (Seller is null)
                throw new ProductWithoutSellerException(this);

            if (Seller.Id != sellerId)
                throw new ProductDoesNotBelongToSellerException(this, Seller);
        }


        public void RemoveFromSale(Seller seller)
        {
            EnsureSeller(seller);
            ListingStatus = ProductListingStatus.Unlisted;
        }

        public void ReturnToSale(Seller seller)
        {
            EnsureSeller(seller);
            ListingStatus = ProductListingStatus.Listed;
        }

        public void SellerIncreaseStock(Seller seller, Quantity additionalQuantity)
        {
            if (additionalQuantity is null)
                throw new ArgumentNullValueException(nameof(additionalQuantity));

            if (additionalQuantity.Value <= 0)
                throw new QuantityMustBePositiveException(this, additionalQuantity);

            EnsureSeller(seller);
            StockQuantity = new Quantity(StockQuantity.Value + additionalQuantity.Value);
        }

        public void SellerDecreaseStock(Seller seller, Quantity additionalQuantity)
        {
            if (additionalQuantity is null)
                throw new ArgumentNullValueException(nameof(additionalQuantity));

            // Строго положительное количество
            if (additionalQuantity.Value <= 0)
                throw new QuantityMustBePositiveException(this, additionalQuantity);

            // Разрешаем уменьшать в ноль, запрещаем только уходить в минус
            if (additionalQuantity.Value > StockQuantity.Value)
                throw new QuantityDecreaseExceedsAvailableException(this, additionalQuantity, StockQuantity);

            EnsureSeller(seller);
            StockQuantity = new Quantity(StockQuantity.Value - additionalQuantity.Value);
        }


        public void OrderRefundStock(Seller seller, Quantity additionalQuantity)
        {
            if (additionalQuantity is null)
                throw new ArgumentNullValueException(nameof(additionalQuantity));

            if (additionalQuantity.Value <= 0)
                throw new QuantityMustBePositiveException(this, additionalQuantity);

            EnsureSeller(seller);
            StockQuantity = new Quantity(StockQuantity.Value + additionalQuantity.Value);
        }

        // Новое: возврат по sellerId
        public void OrderRefundStock(Guid sellerId, Quantity additionalQuantity)
        {
            if (additionalQuantity is null)
                throw new ArgumentNullValueException(nameof(additionalQuantity));

            if (additionalQuantity.Value <= 0)
                throw new QuantityMustBePositiveException(this, additionalQuantity);

            EnsureSellerId(sellerId);
            StockQuantity = new Quantity(StockQuantity.Value + additionalQuantity.Value);
        }

        public void OrderRemoveStock(Seller seller, Quantity additionalQuantity)
        {
            if (additionalQuantity is null)
                throw new ArgumentNullValueException(nameof(additionalQuantity));

            if (additionalQuantity.Value <= 0)
                throw new QuantityMustBePositiveException(this, additionalQuantity);

            if (additionalQuantity.Value > StockQuantity.Value)
                throw new QuantityDecreaseExceedsAvailableException(this, additionalQuantity, StockQuantity);

            EnsureSeller(seller);
            StockQuantity = new Quantity(StockQuantity.Value - additionalQuantity.Value);
        }

        // Новое: списание по sellerId
        public void OrderRemoveStock(Guid sellerId, Quantity additionalQuantity)
        {
            if (additionalQuantity is null)
                throw new ArgumentNullValueException(nameof(additionalQuantity));

            if (additionalQuantity.Value <= 0)
                throw new QuantityMustBePositiveException(this, additionalQuantity);

            if (additionalQuantity.Value > StockQuantity.Value)
                throw new QuantityDecreaseExceedsAvailableException(this, additionalQuantity, StockQuantity);

            EnsureSellerId(sellerId);
            StockQuantity = new Quantity(StockQuantity.Value - additionalQuantity.Value);
        }

        public void ChangePrice(Money newPrice, Seller seller)
        {
            EnsureSeller(seller);

            if (newPrice is null)
                throw new ArgumentNullValueException(nameof(newPrice));

            Price = newPrice;
        }

        #endregion
    }
}
