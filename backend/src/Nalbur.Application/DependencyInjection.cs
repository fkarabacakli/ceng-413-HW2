using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nalbur.Application.Services;

namespace Nalbur.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, StockSettings settings)
    {
        services.AddSingleton(settings);
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
