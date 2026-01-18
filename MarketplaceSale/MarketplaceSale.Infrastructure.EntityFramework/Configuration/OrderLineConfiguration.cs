using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
    {
        public void Configure(EntityTypeBuilder<OrderLine> builder)
        {
            builder.HasKey(ol => ol.Id);

            // Важно: выбери один вариант для Id:
            // - ValueGeneratedNever(), если Id задаётся в домене (Guid.NewGuid())
            // - ValueGeneratedNever(), если Id генерируется в БД/EF
            builder.Property(ol => ol.Id).ValueGeneratedNever();

            builder.HasOne(ol => ol.Product)
                   .WithMany()
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ol => ol.Seller)
                   .WithMany()
                   .HasForeignKey("SellerId")
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // ❌ УДАЛИТЬ этот блок полностью, чтобы не было второй связи:
            // builder.HasOne(ol => ol.Order)
            //        .WithMany("_orderLines")
            //        .HasForeignKey(ol => ol.OrderId)
            //        .IsRequired();

            builder.Property(ol => ol.Quantity)
                   .HasConversion(q => q.Value, v => new Quantity(v))
                   .IsRequired();
        }
    }
}
