using MarketplaceSale.Domain.Entities;
using MarketplaceSale.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class OrderReturnProductConfiguration : IEntityTypeConfiguration<OrderReturnProduct>
    {
        public void Configure(EntityTypeBuilder<OrderReturnProduct> builder)
        {
            builder.ToTable("OrderReturnedProducts");

            // PK по Id (как в домене Entity<Guid>)
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            // FK на Order (навигации нет — это ок)
            builder.HasIndex(x => x.OrderId);

            // Уникальность "по смыслу строки"
            builder.HasIndex(x => new { x.OrderId, x.SellerId, x.ProductId })
                   .IsUnique();

            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.SellerId).IsRequired();
            builder.Property(x => x.ProductId).IsRequired();

            builder.Property(x => x.Quantity)
                   .HasConversion(q => q.Value, v => new Quantity(v))
                   .IsRequired();
        }
    }
}
