using MarketplaceSale.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class SellerConfiguration : IEntityTypeConfiguration<Seller>
    {
        public void Configure(EntityTypeBuilder<Seller> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedNever();

            builder.Property(s => s.Username)
                .IsRequired()
                .HasConversion(
                    u => u.Value,
                    v => new Domain.ValueObjects.Username(v))
                .HasMaxLength(50);

            builder.Property(s => s.BusinessBalance)
                .IsRequired()
                .HasConversion(
                    money => money.Value,
                    value => new Money(value)
                );

            // Навигация к продуктам (Seller -> _products)
            builder.HasMany<Product>("_products")
                .WithOne(p => p.Seller)  // предполагается, что в Product есть свойство Seller
                .HasForeignKey("SellerId")
                .OnDelete(DeleteBehavior.Cascade);

            // Навигация к истории продаж (Seller -> Orders)
            /*/builder.HasMany<Order>("_salesHistory")
                .WithOne(o => o.Seller)  
                .HasForeignKey("SellerId")
                .OnDelete(DeleteBehavior.Restrict);*/

            builder.Navigation("_products")
                .HasField("_products")            // имя поля в классе Seller
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            //builder.Navigation("_salesHistory").UsePropertyAccessMode(PropertyAccessMode.Field);

            // Игнорируем публичные коллекции-обертки, т.к. они не являются сущностями EF
            builder.Ignore(s => s.AvailableProducts);
            //builder.Ignore(s => s.SalesHistory);
        }
    }

}
