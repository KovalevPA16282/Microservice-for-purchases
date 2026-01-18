using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    public class Cart : Entity<Guid>
    {
        #region Fields

        private readonly List<CartLine> _cartLines = new();

        #endregion

        #region Properties

        public Client Client { get; private set; } = null!;

        // Без лишних аллокаций: read-only view поверх списка (без ToList()).
        public IReadOnlyList<CartLine> CartLines => _cartLines.AsReadOnly();

        #endregion

        #region Constructors

        protected Cart() { }

        public Cart(Client client)
            : base(Guid.NewGuid())
        {
            Client = client ?? throw new ArgumentNullValueException(nameof(client));
        }

        #endregion

        #region Behavior

        public void AddProduct(Product product, Quantity quantity)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            var productId = product.Id;
            var line = _cartLines.FirstOrDefault(l => l.ProductId == productId);

            // Новый блок: проверка, что итоговое количество в корзине не больше stock
            var currentQty = line?.Quantity.Value ?? 0;
            var newTotal = currentQty + quantity.Value;

            if (newTotal > product.StockQuantity.Value)
                throw new NotEnoughStockException(product, new Quantity(newTotal));
            // -----------------------------

            if (line != null)
                line.IncreaseQuantity(quantity);
            else
                _cartLines.Add(new CartLine(this, product, quantity));
        }


        public void RemoveProduct(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            var productId = product.Id;

            var line = _cartLines.FirstOrDefault(l => l.ProductId == productId);
            if (line is null)
                throw new ProductNotInCartException(product);

            _cartLines.Remove(line);
        }

        public void SelectAllForBuy()
        {
            foreach (var line in _cartLines)
                line.SelectProduct();
        }

        public void UnselectAllForBuy()
        {
            foreach (var line in _cartLines)
                line.UnselectProduct();
        }

        public void SelectForBuy(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            var productId = product.Id;

            var line = _cartLines.FirstOrDefault(l => l.ProductId == productId);
            if (line is null)
                throw new ProductNotInCartException(product);

            line.SelectProduct();
        }

        public void UnselectForBuy(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            var productId = product.Id;

            var line = _cartLines.FirstOrDefault(l => l.ProductId == productId);
            if (line is null)
                throw new ProductNotInCartException(product);

            line.UnselectProduct();
        }

        public void ClearSelected()
        {
            // Удаляем без промежуточного списка
            _cartLines.RemoveAll(line => line.SelectionStatus == CartSelectionStatus.Selected);
        }

        public void ClearCart() => _cartLines.Clear();

        public Money GetTotalPrice()
        {
            decimal total = 0m;

            foreach (var line in _cartLines)
                total += line.GetPrice().Value;

            return new Money(total);
        }

        #endregion
    }
}
