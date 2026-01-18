using System;
using System.Linq;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.ValueObjects;

namespace ClassTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Создаём Покупательа
                Username username1 = new("Иванов Иван");
                Client client = new(username1);
                Console.WriteLine($"Создан покупатель {client.Username} (ID: {client.Id})\n");

                // Создаём корзину для Покупательа  
                Cart cart = new(client);
                Console.WriteLine($"Создана корзина для покупателя {client.Username} (ID: {client.Id})\n");

                // Создаём продавцов
                Guid sellerId1 = Guid.NewGuid();
                Username username2 = new("Продавец 1");
                Seller seller1 = new(sellerId1, username2);
                Console.WriteLine($"Создан продавец {seller1.Username} (ID: {seller1.Id})\n");

                Guid sellerId2 = Guid.NewGuid();
                Username username3 = new("Продавец 2");
                Seller seller2 = new(sellerId2, username3);
                Console.WriteLine($"Создан продавец {seller2.Username} (ID: {seller2.Id})\n");

                // Создаём продукты
                ProductName phoneName = new("Iphone 16");
                ProductName caseName = new("Чехол для Iphone 16");

                Description phoneDescription = new("Современный смартфон с высокой производительностью и улучшенной камерой.");
                Description caseDescription = new("Прочный и стильный чехол, обеспечивающий надёжную защиту для iPhone 16.");

                Money price1 = new(999);
                Money price2 = new(150);

                Quantity quantity1 = new(10);
                Quantity quantity2 = new(10);

                Product iphone16 = new(phoneName, phoneDescription, price1, quantity1, seller1);
                Product iphoneCase = new(caseName, caseDescription, price2, quantity2, seller2);

                Console.WriteLine($"Создан товар {iphone16.ProductName}\nОписание: {iphone16.Description}\n(ID: {iphone16.Id})\n");
                Console.WriteLine($"Создан товар {iphoneCase.ProductName}\nОписание: {iphoneCase.Description}\n(ID: {iphoneCase.Id})\n");

                seller1.AddProduct(iphone16);
                seller2.AddProduct(iphoneCase);

                Console.WriteLine("Товары добавлены в магазин продавцами.");
                PrintSeller(seller1);
                PrintSeller(seller2);

                //Добавляем товары в корзину
                client.AddToCart(iphone16, new Quantity(3));
                client.AddToCart(iphoneCase, new Quantity(2));
                Console.WriteLine($"\nПокупатель {client.Username} добавил в корзину {iphone16.ProductName} и {iphoneCase.ProductName}");

                // Выводим содержимое корзины
                PrintCart(client);

                client.RemoveFromCart(iphoneCase);
                Console.WriteLine($"Покупатель удалил {iphoneCase.ProductName} из корзины.");

                PrintCart(client);

                // Очищаем корзину
                Console.WriteLine("Очищаем корзину...");
                client.ClearCart();

                PrintCart(client);

                client.AddToCart(iphone16, new Quantity(1));
                client.AddToCart(iphoneCase, new Quantity(1));
                Console.WriteLine($"Покупатель {client.Username} добавил в корзину {iphone16.ProductName} и {iphoneCase.ProductName}");
                PrintCart(client);
                //пополнение счёта
                client.AddBalance(new Money(5000));
                Console.WriteLine("Покупатель пополнил баланс");
                Console.WriteLine($"Баланс покупателя после пополнения: {client.AccountBalance}");

                client.SelectProductForOrder(iphone16);
                client.SelectProductForOrder(iphoneCase);
                client.PlaceSelectedOrderFromCart();
                Console.WriteLine($"Покупатель выбрал {iphone16.ProductName} и {iphoneCase.ProductName} в корзине и создал заказ \n");
                var returnOrder1 = client.PurchaseHistory.Last();
                client.PayForOrder(returnOrder1);
                Console.WriteLine("Покупатель оплатил заказ.");
                Console.WriteLine($"Баланс покупателя после заказа: {client.AccountBalance}");
                Console.WriteLine($"Баланс продавца1 после заказа: {seller1.BusinessBalance}");
                Console.WriteLine($"Баланс продавца2 после заказа: {seller2.BusinessBalance}");

                returnOrder1.MarkAsShipped();
                returnOrder1.MarkAsDelivered();
                returnOrder1.MarkAsCompleted();
                

                PrintCart(client);
                PrintSeller(seller1);
                PrintSeller(seller2);

                client.PlaceDirectOrder(iphoneCase, new Quantity(1));
                Console.WriteLine($"\nПокупатель оформил заказ напрямую.{iphoneCase.ProductName} в количестве: {1}");
                var returnOrder2 = client.PurchaseHistory.Last();
                client.PayForOrder(returnOrder2);
                Console.WriteLine($"Баланс Покупателя после заказа: {client.AccountBalance}");
                Console.WriteLine($"Баланс продавца после заказа: {seller2.BusinessBalance}");
                PrintSeller(seller2);

                client.CancelOrder(returnOrder2);
                Console.WriteLine($"\nПокупатель отменил заказ.{iphoneCase.ProductName} в количестве: {1}");
                Console.WriteLine($"\nСтатус заказа после отмены заказа: {returnOrder2.Status}");

                Console.WriteLine($"Баланс покупателя после заказа: {client.AccountBalance}");
                Console.WriteLine($"Баланс продавца2 после заказа: {seller2.BusinessBalance}");
                PrintSeller(seller2);

                // Покупатель подаёт запрос на возврат
                client.RequestProductReturn(returnOrder1, iphone16, new Quantity(1));
                Console.WriteLine($"\nПокупатель создал запрос на возврат товара {iphone16.ProductName}");

                // Продавец 1 соглашается на возврат
                seller1.ApproveOrderReturn(returnOrder1);
                Console.WriteLine($"\n{seller1.Username} одобрил возврат товара {iphone16.ProductName}");
                
                client.RequestProductReturn(returnOrder1, iphoneCase, new Quantity(1));
                Console.WriteLine($"\nПокупатель создал запрос на возврат товара {iphoneCase.ProductName}");

                // Продавец 2 отклоняет возврат
                seller2.RejectOrderReturn(returnOrder1);
                Console.WriteLine($"\n{seller2.Username} отклонил возврат товара {iphoneCase.ProductName}");


                Console.WriteLine("\nСтатусы возврата:");
                foreach (var sellerStatus in returnOrder1.ReturnStatuses)
                {
                    var seller = sellerStatus.Key;
                    var status = sellerStatus.Value;
                    var productsFromSeller = returnOrder1.OrderLines
                        .Where(line => line.Product.Seller.Id == seller)
                        .Select(line => line.Product);

                    foreach (var product in productsFromSeller)
                    {
                        Console.WriteLine($"- Продукт: {product.ProductName}, Статус: {status}");
                    }
                }


                Console.WriteLine($"\nБаланс покупателя после возвратов: {client.AccountBalance}");
                Console.WriteLine($"\nБаланс продавца1 после возврата: {seller1.BusinessBalance}");
                PrintSeller(seller1);
                Console.WriteLine($"\nБаланс продавца2 после возврата: {seller2.BusinessBalance}");
                PrintSeller(seller2);

                ////потом менять статусы типа доставка, после деливер сделать два возврата, в одном продавец соглашается, в другом нет
                //var lastOrder = client.PurchaseHistory.Last();

                //Console.WriteLine($"\nСтатус заказа после оплаты: {lastOrder.Status}");


                //client.CancelOrder(lastOrder);
                //Console.WriteLine($"\nСтатус заказа после отмены заказа: {lastOrder.Status}");

                //Console.WriteLine($"Баланс покупателя после заказа: {client.AccountBalance}");
                //Console.WriteLine($"Баланс продавца1 после заказа: {seller1.BusinessBalance}");
                //Console.WriteLine($"Баланс продавца2 после заказа: {seller2.BusinessBalance}");
                //PrintCart(client);
                //PrintSeller(seller1);
                //PrintSeller(seller2);

                /*
                // Отправляем заказ
                lastOrder.MarkAsShipped();
                Console.WriteLine($"Статус заказа после отправки: {lastOrder.Status}");

                // Доставляем заказ
                lastOrder.MarkAsDelivered();
                Console.WriteLine($"Статус заказа после доставки: {lastOrder.Status}");
                Console.WriteLine($"Дата доставки: {lastOrder.DeliveryDate}");

                // Завершаем заказ
                lastOrder.MarkAsCompleted();
                Console.WriteLine($"Статус заказа после завершения: {lastOrder.Status}");
                */

                // Снова оформляем заказ
                //client.AddToCart(iphone16, new Quantity(1));
                //client.AddToCart(iphoneCase, new Quantity(1));
                //client.SelectProductForOrder(iphone16);
                //client.SelectProductForOrder(iphoneCase);
                ////client.UnselectProductForOrder(iphoneCase);
                //client.PlaceSelectedOrderFromCart();

                //var returnOrder = client.PurchaseHistory.Last();
                //client.PayForOrder(returnOrder);

                //// Доставка заказа
                //returnOrder.MarkAsShipped();
                //returnOrder.MarkAsDelivered();
                //returnOrder.MarkAsCompleted();


                //// Покупатель подаёт запрос на возврат
                //client.RequestProductReturn(returnOrder, iphone16, new Quantity(1));
                //Console.WriteLine("\nЗапрос на возврат");

                //// Продавец 1 соглашается на возврат
                //seller1.ApproveOrderReturn(returnOrder);
                //Console.WriteLine($"Статус заказа после завершения: {returnOrder.Status}");

                //client.RequestProductReturn(returnOrder, iphoneCase, new Quantity(1));
                //Console.WriteLine("\nЗапрос на возврат");

                //// Продавец 2 отклоняет возврат
                //seller2.RejectOrderReturn(returnOrder);

                //Console.WriteLine($"Статус заказа после завершения: {returnOrder.Status}");


                //Console.WriteLine("\nСтатусы возврата:");
                //foreach (var sellerStatus in returnOrder.ReturnStatuses)
                //{
                //    var seller = sellerStatus.Key;
                //    var status = sellerStatus.Value;
                //    var productsFromSeller = returnOrder.OrderLines
                //        .Where(line => line.Product.Seller == seller)
                //        .Select(line => line.Product);

                //    foreach (var product in productsFromSeller)
                //    {
                //        Console.WriteLine($"- Продукт: {product.ProductName}, Статус: {status}");
                //    }
                //}


                //Console.WriteLine($"\nБаланс покупателя после возвратов: {client.AccountBalance}");
                //Console.WriteLine($"\nБаланс продавца1 после возврата: {seller1.BusinessBalance}");
                //PrintSeller(seller1);
                //Console.WriteLine($"\nБаланс продавца2 после возврата: {seller2.BusinessBalance}");
                //PrintSeller(seller2);

                seller1.RemoveProduct(iphone16);
                client.AddToCart(iphone16, new Quantity(1));

                PrintCart(client);

                // Локальная функция для вывода корзины
                void PrintCart(Client client)
                {
                    Console.WriteLine($"\nКорзина покупателя {client.Username}:");
                    if (!client.Cart.CartLines.Any())
                    {
                        Console.WriteLine("Корзина пуста.\n");
                        return;
                    }

                    foreach (var line in client.Cart.CartLines)
                    {
                        Console.WriteLine($"- {line.Product.ProductName} x {line.Quantity} = {line.GetPrice()}");
                    }

                    Console.WriteLine($"Общая сумма: {client.Cart.GetTotalPrice()}\n");
                }

                void PrintSeller(Seller seller)
                {
                    Console.WriteLine($"\nИнвентарь продавца: {seller.Username}");

                    var availableProducts = seller.AvailableProducts;

                    if (!availableProducts.Any())
                    {
                        Console.WriteLine("Нет доступных товаров.");
                        return;
                    }

                    foreach (var product in availableProducts)
                    {
                        Console.WriteLine($"- {product.ProductName} | Остаток: {product.StockQuantity} | Цена: {product.Price}");
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
            
        }
    }
}
