using MarketplaceSale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarketplaceSale.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketplaceSale.Infrastructure.EntityFramework.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.ProductName)
                .IsRequired()
                .HasConversion(
                    pn => pn.Value,
                    v => new ProductName(v))
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasConversion(
                    d => d.Value,
                    v => new Description(v))
                .HasMaxLength(500);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasConversion(
                    money => money.Value,
                    value => new Money(value));

            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasConversion(
                    q => q.Value,
                    value => new Quantity(value));

            builder.HasOne(p => p.Seller)
                .WithMany("_products")
                .HasForeignKey("SellerId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property<Guid>("SellerId");

        }
    }

}
