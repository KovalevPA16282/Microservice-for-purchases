using MarketplaceSale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarketplaceSale.Domain.ValueObjects;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class CartLineConfiguration : IEntityTypeConfiguration<CartLine>
    {
        public void Configure(EntityTypeBuilder<CartLine> builder)
        {
            builder.HasKey(cl => cl.Id);
            builder.Property(cl => cl.Id).ValueGeneratedNever();

            builder.HasOne(cl => cl.Cart)
                .WithMany(c => c.CartLines)
                .HasForeignKey("CartId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cl => cl.Product)
                .WithMany()
                .HasForeignKey("ProductId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(cl => cl.Quantity)
                .IsRequired()
                .HasConversion(
                    q => q.Value,
                    v => new Quantity(v));

            builder.Property(cl => cl.SelectionStatus)
                .IsRequired()
                .HasConversion<int>();
        }
    }
}


