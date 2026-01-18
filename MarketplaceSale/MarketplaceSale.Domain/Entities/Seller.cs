using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    public class Seller(Guid id, Username username) : Entity<Guid>(id)
    {
        #region Fields

        private readonly ICollection<Product> _products = new List<Product>();

        #endregion

        #region Properties

        public Username Username { get; private set; } =
            username ?? throw new ArgumentNullValueException(nameof(username));

        public Money BusinessBalance { get; private set; } = new(0);

        public IReadOnlyCollection<Product> AvailableProducts =>
            _products
                .Where(p => p.ListingStatus == ProductListingStatus.Listed &&
                            p.StockQuantity.Value > 0)
                .ToList()
                .AsReadOnly();

        #endregion

        #region Behavior

        public bool ChangeUsername(Username newUsername)
        {
            if (newUsername is null)
                throw new ArgumentNullValueException(nameof(newUsername));

            if (Username == newUsername) return false;
            Username = newUsername;
            return true;
        }

        public void AddProduct(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (product.Seller is null || product.Seller.Id != this.Id)
                product.AssignToSeller(this);

            if (_products.Any(p => p.Id == product.Id))
                throw new ProductAlreadyExistsException(this, product);

            _products.Add(product);
        }

        public void RemoveProduct(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            EnsureOwnership(product);
            product.RemoveFromSale(this);
        }

        public void ReturnProductToSale(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            EnsureOwnership(product);
            product.ReturnToSale(this);
        }

        public void DeleteProduct(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            EnsureOwnership(product);

            if (product.StockQuantity.Value > 0)
                throw new InvalidOperationException("Cannot delete product while stock is greater than zero.");

            product.RemoveFromSale(this);

            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
                _products.Remove(existing);
        }

        public void ReplenishProduct(Product product, Quantity quantity)
        {
            EnsureOwnership(product);
            product.SellerIncreaseStock(this, quantity);
        }

        public void ReduceProductStock(Product product, Quantity quantity)
        {
            EnsureOwnership(product);
            product.SellerDecreaseStock(this, quantity);
        }

        public void ChangeProductPrice(Product product, Money newPrice)
        {
            EnsureOwnership(product);
            product.ChangePrice(newPrice, this);
        }

        public void RejectOrderReturn(Order order)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            EnsureOrderHasThisSeller(order);

            order.RejectReturn(this);
        }

        public void ApproveOrderReturn(Order order)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            EnsureOrderHasThisSeller(order);

            decimal refundAmount = 0m;

            foreach (var line in order.OrderLines.Where(l => l.SellerId == this.Id))
            {
                var key = (SellerId: this.Id, ProductId: line.Product.Id);

                if (!order.ReturnedProducts.TryGetValue(key, out var qty))
                    continue;

                if (qty.Value <= 0)
                    continue;

                if (qty.Value > line.Quantity.Value)
                    throw new InvalidRefundQuantityException(line.Product, qty, line.Quantity.Value);

                refundAmount += line.Product.Price.Value * qty.Value;
            }

            if (refundAmount <= 0m)
                throw new InvalidOperationException("No return items found for this seller.");

            var money = new Money(refundAmount);

            // 1) Сначала убедиться, что продавец реально может отдать деньги (может бросить NotEnoughFundsException)
            SubtractBalance(money);

            // 2) Теперь можно начислять клиенту (это уже не должно бросать по текущим правилам Money)
            order.Client.AddBalance(money);

            // 3) И только после успешных денег — менять состояние возврата и делать stock-refund внутри заказа
            order.ApproveReturn(this);
            order.MarkAsRefunded(this);
        }


        private void EnsureOwnership(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (product.Seller is null ||
                product.Seller.Id != this.Id ||
                !_products.Any(p => p.Id == product.Id))
            {
                throw new ProductDoesNotBelongToSellerException(product, this);
            }
        }

        private void EnsureOrderHasThisSeller(Order order)
        {
            // Источник истины: наличие строк этого продавца в заказе
            if (!order.OrderLines.Any(l => l.SellerId == this.Id))
                throw new OrderDoesNotBelongToSellerException(order, this);
        }

        public void AddBalance(Money amount)
        {
            if (amount is null)
                throw new ArgumentNullValueException(nameof(amount));

            BusinessBalance += amount;
        }

        public void SubtractBalance(Money amount)
        {
            if (amount is null)
                throw new ArgumentNullValueException(nameof(amount));

            if (amount > BusinessBalance)
                throw new NotEnoughFundsException(BusinessBalance, amount);

            BusinessBalance -= amount;
        }

        #endregion
    }
}
