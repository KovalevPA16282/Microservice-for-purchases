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
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            builder.Property(c => c.Username)
                .IsRequired()
                .HasConversion(
                    u => u.Value,
                    v => new Username(v))
                .HasMaxLength(50);

            builder.Property(c => c.AccountBalance)
                .IsRequired()
                .HasConversion(
                    m => m.Value,
                    v => new Money(v));

            builder.HasOne(c => c.Cart)
                .WithOne(cart => cart.Client)
                .HasForeignKey<Cart>("ClientId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<Order>("_purchaseHistory")
                .WithOne(o => o.Client)
                .HasForeignKey("ClientId")
                .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany<Order>("_returnHistory")
            //    .WithOne(o => o.ClientReturning)
            //    .HasForeignKey("ClientReturningId")
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation("_purchaseHistory").UsePropertyAccessMode(PropertyAccessMode.Field);
            //builder.Navigation("_returnHistory").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(c => c.PurchaseHistory);
            //builder.Ignore(c => c.ReturnHistory);

        }
    }

}
