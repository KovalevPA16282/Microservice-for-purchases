using Microsoft.EntityFrameworkCore;
using MarketplaceSale.Domain.Entities;

namespace MarketplaceSale.Infrastructure.EntityFramework
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartLine> CartLines { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderLine> OrderLines { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Seller> Sellers { get; set; } = null!;

        // ✅ Теперь это доменные сущности, которые EF хранит в таблицах:
        public DbSet<OrderReturnProduct> OrderReturnedProducts { get; set; } = null!;
        public DbSet<OrderReturnStatus> OrderReturnStatuses { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // пример: читаешь из переменной окружения / конфигурации
            // optionsBuilder.EnableSensitiveDataLogging(isDevelopment);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}
