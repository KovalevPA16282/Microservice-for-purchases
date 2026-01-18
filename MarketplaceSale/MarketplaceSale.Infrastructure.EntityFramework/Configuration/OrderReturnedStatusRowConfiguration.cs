using MarketplaceSale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class OrderReturnStatusConfiguration : IEntityTypeConfiguration<OrderReturnStatus>
    {
        public void Configure(EntityTypeBuilder<OrderReturnStatus> builder)
        {
            builder.ToTable("OrderReturnStatuses");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => new { x.OrderId, x.SellerId })
                   .IsUnique();

            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.SellerId).IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<int>()
                   .IsRequired();
        }
    }
}
