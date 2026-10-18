using Nalbur.Application.Services;

namespace Nalbur.Api.Endpoints;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        // Latest movements across all products: GET /api/movements?take=20
        app.MapGet("/api/movements", async (int? take, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetRecentMovementsAsync(take ?? 20, ct)));

        // Numbers for the dashboard header
        app.MapGet("/api/summary", async (IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetSummaryAsync(ct)));

        return app;
    }
}
