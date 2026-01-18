using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.WebHost.Helpers
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase<T>(this IHost host) where T : DbContext
        {
            var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<T>();
            dbContext?.Database.Migrate();
            return host;
        }
    }
}
