using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Enums;
using MarketplaceSale.Domain.Exceptions;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Domain.Entities
{
    public class Order : Entity<Guid>
    {
        #region Fields

        private readonly List<OrderLine> _orderLines = new();

        // Табличное хранение возвратов (для EF и домена)
        private readonly List<OrderReturnProduct> _returnedProductsRows = new();
        private readonly List<OrderReturnStatus> _returnStatusesRows = new();

        #endregion

        #region Properties

        private Order() { }

        public Client Client { get; private set; } = null!;

        //public Client? ClientReturning { get; private set; }
        ////public Client? ClientHistory { get; private set; }
        //public Guid? ClientReturningId { get; private set; }

        public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();

        // Проекции поверх rows, чтобы остальной код мог работать со словарями
        public IReadOnlyDictionary<(Guid SellerId, Guid ProductId), Quantity> ReturnedProducts =>
            _returnedProductsRows.ToDictionary(
                x => (x.SellerId, x.ProductId),
                x => x.Quantity);

        public IReadOnlyDictionary<Guid, ReturnStatus> ReturnStatuses =>
            _returnStatusesRows.ToDictionary(
                x => x.SellerId,
                x => x.Status);

        public Money TotalAmount { get; private set; } = new Money(0);
        public OrderStatus Status { get; private set; }
        public OrderDate OrderDate { get; private set; } = null!;
        public DeliveryDate? DeliveryDate { get; private set; }

        #endregion

        #region Constructors

        public Order(Client client, IEnumerable<Product> products)
            : base(Guid.NewGuid())
        {
            if (client is null)
                throw new ArgumentNullValueException(nameof(client));

            if (products is null || !products.Any())
                throw new EmptyOrderProductListException();

            Client = client;
            Status = OrderStatus.Pending;
            OrderDate = new OrderDate(DateTime.UtcNow);

            foreach (var group in products.GroupBy(p => p.Id))
            {
                var product = group.First();
                var line = new OrderLine(product, new Quantity(group.Count()));
                line.AttachToOrder(this);
                _orderLines.Add(line);
            }

            RecalculateTotal();
        }

        #endregion

        #region Behavior

        public Money CalculateTotal()
            => new Money(_orderLines.Sum(l => l.Product.Price.Value * l.Quantity.Value));

        private void RecalculateTotal()
            => TotalAmount = CalculateTotal();

        public void AddProduct(Product product, Quantity quantity)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            var line = _orderLines.FirstOrDefault(l => l.Product.Id == product.Id);

            if (line != null)
            {
                line.IncreaseQuantity(quantity);
            }
            else
            {
                var newLine = new OrderLine(product, quantity);
                newLine.AttachToOrder(this);
                _orderLines.Add(newLine);
            }

            RecalculateTotal();
        }

        public void RemoveProduct(Product product)
        {
            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            var line = _orderLines.FirstOrDefault(l => l.Product.Id == product.Id);
            if (line == null)
                throw new ProductNotInCartException(product);

            _orderLines.Remove(line);
            RecalculateTotal();
        }

        public void MarkAsPending()
        {
            if (Status == OrderStatus.Paid)
                throw new InvalidOrderStatusChangeException(Status, OrderStatus.Paid);

            Status = OrderStatus.Pending;
        }

        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOrderStatusChangeException(Status, OrderStatus.Paid);

            Status = OrderStatus.Paid;
        }

        public void MarkAsShipped()
        {
            if (Status != OrderStatus.Paid)
                throw new InvalidOrderStatusChangeException(Status, OrderStatus.Shipped);

            Status = OrderStatus.Shipped;
        }

        public void MarkAsDelivered()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOrderStatusChangeException(Status, OrderStatus.Delivered);

            Status = OrderStatus.Delivered;
            DeliveryDate = new DeliveryDate(DateTime.UtcNow);
        }

        public void MarkAsCompleted()
        {
            if (Status != OrderStatus.Delivered)
                throw new InvalidOrderStatusChangeException(Status, OrderStatus.Completed);

            Status = OrderStatus.Completed;
        }

        public void MarkAsCancelled()
        {
            if (Status != OrderStatus.Paid)
                throw new InvalidOrderCancellationException(Status);

            Status = OrderStatus.Cancelled;
        }

        /// <summary>
        /// Клиент запрашивает возврат части товара у конкретного продавца.
        /// </summary>
        public void RequestProductReturn(Seller seller, Product product, Quantity quantity)
        {
            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            if (product is null)
                throw new ArgumentNullValueException(nameof(product));

            if (quantity is null || quantity.Value <= 0)
                throw new QuantityMustBePositiveException(product, quantity);

            if (Status != OrderStatus.Completed)
                throw new InvalidReturnRequestException(Status);

            var sellerId = seller.Id;

            // Ищем строку заказа по ProductId + SellerId (через OrderLine.SellerId)
            var orderLine = _orderLines.FirstOrDefault(line =>
                line.Product.Id == product.Id &&
                line.SellerId == sellerId);

            if (orderLine is null)
                throw new ProductNotInOrderException(product);

            var currentStatus = _returnStatusesRows
                .FirstOrDefault(x => x.SellerId == sellerId)?.Status
                ?? ReturnStatus.None;

            if (currentStatus != ReturnStatus.None)
                throw new ReturnAlreadyInProgressException(currentStatus);


            var existingRow = _returnedProductsRows.FirstOrDefault(x =>
                x.SellerId == sellerId &&
                x.ProductId == product.Id);

            var alreadyRequested = existingRow?.Quantity.Value ?? 0;
            var newTotalRequested = alreadyRequested + quantity.Value;

            if (newTotalRequested > orderLine.Quantity.Value)
                throw new InvalidRefundQuantityException(
                    product,
                    new Quantity(newTotalRequested),
                    orderLine.Quantity.Value);

            if (existingRow is null)
            {
                _returnedProductsRows.Add(
                    new OrderReturnProduct(
                        Id,
                        sellerId,
                        product.Id,
                        new Quantity(newTotalRequested)));
            }
            else
            {
                existingRow.ChangeQuantity(new Quantity(newTotalRequested));
            }

            var statusRow = _returnStatusesRows.FirstOrDefault(x => x.SellerId == sellerId);
            if (statusRow is null)
            {
                _returnStatusesRows.Add(
                    new OrderReturnStatus(
                        Id,
                        sellerId,
                        ReturnStatus.Requested));
            }
            else
            {
                statusRow.ChangeStatus(ReturnStatus.Requested);
            }
        }

        public void RejectReturn(Seller seller)
        {
            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            var sellerId = seller.Id;

            var statusRow = _returnStatusesRows.FirstOrDefault(x => x.SellerId == sellerId);
            if (statusRow is null || statusRow.Status != ReturnStatus.Requested)
                throw new ReturnNotRequestedException();

            statusRow.ChangeStatus(ReturnStatus.Rejected);

            _returnedProductsRows.RemoveAll(x => x.SellerId == sellerId);
        }

        /// <summary>
        /// Продавец одобряет возврат. Здесь только статус, без склада/денег.
        /// </summary>
        public void ApproveReturn(Seller seller)
        {
            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            var sellerId = seller.Id;

            var statusRow = _returnStatusesRows.FirstOrDefault(x => x.SellerId == sellerId);
            if (statusRow is null || statusRow.Status != ReturnStatus.Requested)
                throw new ReturnNotRequestedException();

            statusRow.ChangeStatus(ReturnStatus.Approved);
        }

        /// <summary>
        /// Возврат завершён: товары реально вернулись на склад (по запрошенному количеству).
        /// Деньги списываются у продавца в Seller.ApproveOrderReturn.
        /// </summary>
        public void MarkAsRefunded(Seller seller)
        {
            if (seller is null)
                throw new ArgumentNullValueException(nameof(seller));

            var sellerId = seller.Id;

            var statusRow = _returnStatusesRows.FirstOrDefault(x => x.SellerId == sellerId);
            if (statusRow is null || statusRow.Status != ReturnStatus.Approved)
                throw new ReturnNotApprovedException();

            foreach (var line in _orderLines.Where(l => l.SellerId == sellerId))
            {
                var row = _returnedProductsRows.FirstOrDefault(x =>
                    x.SellerId == sellerId &&
                    x.ProductId == line.Product.Id);

                if (row is null)
                    continue;

                var requestedQty = row.Quantity;

                if (requestedQty.Value <= 0)
                    continue;

                if (requestedQty.Value > line.Quantity.Value)
                    throw new InvalidRefundQuantityException(
                        line.Product,
                        requestedQty,
                        line.Quantity.Value);

                line.Product.OrderRefundStock(sellerId, requestedQty);
            }

            statusRow.ChangeStatus(ReturnStatus.Refunded);
            //_returnedProductsRows.RemoveAll(x => x.SellerId == sellerId);
        }

        public DeliveryDate GetDeliveryDateOrThrow()
            => DeliveryDate ?? throw new InvalidOperationException("Order is not delivered yet.");

        #endregion
    }
}
