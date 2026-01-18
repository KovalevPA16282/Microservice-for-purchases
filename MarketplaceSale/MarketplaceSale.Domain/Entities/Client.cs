using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    public class Client : Entity<Guid>
    {
        #region Fields

        private readonly List<Order> _purchaseHistory = new();

        #endregion

        #region Properties

        public Username Username { get; private set; }
        public Money AccountBalance { get; private set; } = new(0);
        public Cart Cart { get; private set; }

        public IReadOnlyCollection<Order> PurchaseHistory => _purchaseHistory.AsReadOnly();

        #endregion

        #region Constructors

        protected Client() { }

        public Client(Username username)
            : this(Guid.NewGuid(), username) { }

        protected Client(Guid id, Username username)
            : base(id)
        {
            Username = username ?? throw new ArgumentNullValueException(nameof(username));
            AccountBalance = new Money(0);
            Cart = new Cart(this);
        }

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

        public Money CheckBalance() => AccountBalance;

        public void AddToCart(Product product, Quantity quantity)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            if (product.ListingStatus != ProductListingStatus.Listed)
                throw new ProductWithoutSellerException(product);

            Cart.AddProduct(product, quantity);
        }

        public void RemoveFromCart(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            bool productExistsInCart = Cart.CartLines.Any(cl => cl.ProductId == product.Id);
            if (!productExistsInCart)
                throw new ProductNotFoundInCartException(product);

            Cart.RemoveProduct(product);
        }

        public void ClearCart() => Cart.ClearCart();

        public void AddBalance(Money amount)
        {
            if (amount is null || amount.Value <= 0)
                throw new InvalidMoneyOperationException(amount);

            AccountBalance += amount;
        }

        public void SubtractBalance(Money amount)
        {
            if (amount is null || amount.Value <= 0)
                throw new InvalidMoneyOperationException(amount);

            if (amount > AccountBalance)
                throw new NotEnoughFundsException(AccountBalance, amount);

            AccountBalance -= amount;
        }

        #endregion

        #region Ordering

        public void SelectProductForOrder(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            var line = Cart.CartLines.FirstOrDefault(cl => cl.ProductId == product.Id);
            if (line is null)
                throw new ProductNotFoundInCartException(product);

            line.SelectProduct();
        }

        public void UnselectProductForOrder(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            var line = Cart.CartLines.FirstOrDefault(cl => cl.ProductId == product.Id);
            if (line is null)
                throw new ProductNotFoundInCartException(product);

            line.UnselectProduct();
        }

        public Order PlaceSelectedOrderFromCart()
        {
            var selectedLines = Cart.CartLines
                .Where(line => line.SelectionStatus == CartSelectionStatus.Selected)
                .ToList();

            if (!selectedLines.Any())
                throw new CartSelectionEmptyException();

            foreach (var line in selectedLines)
            {
                if (line.Quantity is null || line.Quantity.Value <= 0)
                    throw new QuantityMustBePositiveException(line.Product, line.Quantity);

                if (line.Product.StockQuantity.Value < line.Quantity.Value)
                    throw new NotEnoughStockException(line.Product, line.Quantity);
            }

            var selectedProducts = new List<Product>();
            foreach (var line in selectedLines)
            {
                for (int i = 0; i < line.Quantity!.Value; i++)
                    selectedProducts.Add(line.Product);
            }

            var totalMoney = new Money(selectedProducts.Sum(p => p.Price.Value));
            if (totalMoney > AccountBalance)
                throw new NotEnoughFundsException(AccountBalance, totalMoney);

            var order = new Order(this, selectedProducts);
            order.MarkAsPending();

            _purchaseHistory.Add(order);
            Cart.ClearSelected();

            return order;
        }

        public Order PlaceDirectOrder(Product product, Quantity quantity)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            if (product.StockQuantity.Value < quantity.Value)
                throw new NotEnoughStockException(product, quantity);

            var products = Enumerable.Repeat(product, quantity.Value).ToList();
            var order = new Order(this, products);

            order.MarkAsPending();
            _purchaseHistory.Add(order);

            return order;
        }

        public void PayForOrder(Order order)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            if (order.Client.Id != this.Id)
                throw new UnauthorizedOrderAccessException(this, order);

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOrderStatusChangeException(order.Status, OrderStatus.Pending);

            var total = order.CalculateTotal();
            if (total > AccountBalance)
                throw new NotEnoughFundsException(AccountBalance, total);

            SubtractBalance(total);

            foreach (var line in order.OrderLines)
            {
                line.Product.OrderRemoveStock(line.SellerId, line.Quantity);

                if (line.Seller.Id != line.SellerId)
                    throw new InvalidOperationException("OrderLine.Seller does not match OrderLine.SellerId.");

                line.Seller.AddBalance(new Money(line.Product.Price.Value * line.Quantity.Value));
            }

            order.MarkAsPaid();
        }

        public void CancelOrder(Order order)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            if (order.Client.Id != this.Id)
                throw new UnauthorizedOrderAccessException(this, order);

            if (order.Status != OrderStatus.Paid)
                throw new InvalidOrderCancellationException(order.Status);

            foreach (var line in order.OrderLines)
            {
                line.Product.OrderRefundStock(line.SellerId, line.Quantity);

                if (line.Seller.Id != line.SellerId)
                    throw new InvalidOperationException("OrderLine.Seller does not match OrderLine.SellerId.");

                line.Seller.SubtractBalance(new Money(line.Product.Price.Value * line.Quantity.Value));
            }

            AddBalance(order.CalculateTotal());
            order.MarkAsCancelled();
        }

        /// <summary>
        /// Старый метод: работает, но может быть ambiguous при одинаковом ProductId у разных продавцов.
        /// Оставляем как есть (с проверкой) — он вызывает Order.RequestProductReturn через seller из OrderLine.
        /// </summary>
        public void RequestProductReturn(Order order, Product product, Quantity quantity)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            if (order.Client.Id != this.Id)
                throw new UnauthorizedOrderAccessException(this, order);

            if (order.Status != OrderStatus.Completed)
                throw new InvalidReturnRequestException(order.Status);

            var matchingLines = order.OrderLines
                .Where(ol => ol.Product.Id == product.Id)
                .ToList();

            if (!matchingLines.Any())
                throw new ProductNotInOrderException(product);

            var distinctSellerCount = matchingLines.Select(l => l.SellerId).Distinct().Count();
            if (distinctSellerCount > 1)
                throw new InvalidOperationException(
                    "Ambiguous return request: same ProductId exists for multiple sellers. Specify seller explicitly.");

            var orderLine = matchingLines[0];

            if (quantity.Value > orderLine.Quantity.Value)
                throw new InvalidRefundQuantityException(product, quantity, orderLine.Quantity.Value);

            if (orderLine.Seller is null || orderLine.Seller.Id != orderLine.SellerId)
                throw new InvalidOperationException("OrderLine.Seller does not match OrderLine.SellerId.");

            order.RequestProductReturn(orderLine.Seller, product, quantity);
        }

        /// <summary>
        /// Новый overload: если UI/сервис знает seller, можно вызывать без ambiguous-проверок.
        /// </summary>
        public void RequestProductReturn(Order order, Seller seller, Product product, Quantity quantity)
        {
            if (order is null)
                throw new ArgumentNullValueException(nameof(order));

            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            if (order.Client.Id != this.Id)
                throw new UnauthorizedOrderAccessException(this, order);

            order.RequestProductReturn(seller, product, quantity);
        }

        #endregion
    }
}
