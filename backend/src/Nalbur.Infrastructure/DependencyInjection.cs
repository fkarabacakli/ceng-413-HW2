using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nalbur.Domain.Repositories;
using Nalbur.Infrastructure.Persistence;
using Nalbur.Infrastructure.Seed;

namespace Nalbur.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NalburDbContext>(options => options.UseNpgsql(connectionString));

        // Scoped: one DbContext and one repository per HTTP request.
        services.AddScoped<IProductRepository, EfProductRepository>();

        // /health fails when PostgreSQL cannot be reached (Database.CanConnectAsync).
        services.AddHealthChecks().AddDbContextCheck<NalburDbContext>("database");
        return services;
    }

    /// <summary>
    /// Applies pending migrations (creates the tables on first start), then optionally
    /// loads the sample data. Seeding is skipped when the database already has products.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seedSampleData, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        await provider.GetRequiredService<NalburDbContext>().Database.MigrateAsync(ct);

        if (seedSampleData)
        {
            await SampleData.SeedAsync(
                provider.GetRequiredService<IProductRepository>(),
                provider.GetRequiredService<TimeProvider>(),
                ct);
        }
    }
}
