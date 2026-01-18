using System;
using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();

            builder.HasOne(o => o.Client)
                   .WithMany("_purchaseHistory")
                   .HasForeignKey("ClientId")
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // ❌ УДАЛИТЬ ЭТИ 5 СТРОК:
            // builder.HasOne(o => o.ClientReturning)
            //        .WithMany("_returnHistory")
            //        .HasForeignKey("ClientReturningId")
            //        .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.TotalAmount)
                   .HasConversion(money => money.Value, value => new Money(value))
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(o => o.Status)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(o => o.OrderDate)
                   .HasConversion(od => od.Value, value => new OrderDate(value))
                   .IsRequired();

            builder.Property(o => o.DeliveryDate)
                   .HasConversion(
                       dd => dd == null ? (DateTime?)null : dd.Value,
                       value => value == null ? null : new DeliveryDate(value.Value))
                   .IsRequired(false);

            // ✅ OrderLines: только одна навигация через публичное свойство
            builder.HasMany(o => o.OrderLines)
                   .WithOne(ol => ol.Order)
                   .HasForeignKey(ol => ol.OrderId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // ✅ Явно указываем backing field для этой навигации
            builder.Navigation(o => o.OrderLines)
                   .HasField("_orderLines")
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Return rows (field-only — нет публичной навигации)
            builder.HasMany<OrderReturnProduct>("_returnedProductsRows")
                   .WithOne()
                   .HasForeignKey(x => x.OrderId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation("_returnedProductsRows")
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany<OrderReturnStatus>("_returnStatusesRows")
                   .WithOne()
                   .HasForeignKey(x => x.OrderId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation("_returnStatusesRows")
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(o => o.ReturnedProducts);
            builder.Ignore(o => o.ReturnStatuses);

            builder.HasIndex("ClientId");
            // ❌ УДАЛИТЬ: builder.HasIndex("ClientReturningId");
        }
    }
}
