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
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            builder.Property<Guid>("ClientId");

            builder.HasOne(c => c.Client)
                .WithOne(client => client.Cart)
                .HasForeignKey<Cart>("ClientId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex("ClientId").IsUnique();


            builder.HasMany(c => c.CartLines)
                .WithOne(cl => cl.Cart)
                .HasForeignKey("CartId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(c => c.CartLines)
                .UsePropertyAccessMode(PropertyAccessMode.Field); 
 
        }
    }

}
