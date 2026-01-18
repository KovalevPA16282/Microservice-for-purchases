using MarketplaceSale.Application.Services;
using MarketplaceSale.Application.Services.Abstractions;
using MarketplaceSale.Application.Services.Mapping;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using MarketplaceSale.Infrastructure.Repositories.Implementation.EF;
using MarketplaceSale.WebHost.Helpers;
using MarketplaceSale.WebHost.Mapping;

namespace MarketplaceSale.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var dbConnectionString = builder.Configuration.GetConnectionString(nameof(ApplicationDbContext));

            if (string.IsNullOrEmpty(dbConnectionString))
            {
                throw new InvalidOperationException("Connection string for ApplicationDbContext is not configured.");
            }

            // Регистрация БД
            builder.Services.AddNpgsql<ApplicationDbContext>(dbConnectionString, options =>
            {
                options.MigrationsAssembly("MarketplaceSale.Infrastructure.EntityFramework");
            });

            // Регистрация AutoMapper
            builder.Services.AddAutoMapper(typeof(PresentationProfile), typeof(ApplicationProfile));

            // Регистрация репозиториев
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderLineRepository, OrderLineRepository>(); // <-- ДОБАВЛЕНО
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<ISellerRepository, SellerRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<ICartLineRepository, CartLineRepository>();

            // Регистрация Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Регистрация Application Services
            builder.Services.AddScoped<ICartApplicationService, CartApplicationService>();
            builder.Services.AddScoped<ICartLineApplicationService, CartLineApplicationService>();
            builder.Services.AddScoped<IClientApplicationService, ClientApplicationService>();
            builder.Services.AddScoped<IOrderApplicationService, OrderApplicationService>();
            builder.Services.AddScoped<IProductApplicationService, ProductApplicationService>();
            builder.Services.AddScoped<ISellerApplicationService, SellerApplicationService>();

            // Контроллеры и документация
            builder.Services.AddControllers();
            builder.Services.AddRazorPages();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();

            // Применяем миграции БД автоматически при запуске
            app.MigrateDatabase<ApplicationDbContext>();

            app.Run();
        }
    }
}
