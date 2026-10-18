using Nalbur.Application.Dtos;
using Nalbur.Application.Services;

namespace Nalbur.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products");

        // GET /api/products?search=vida&critical=true
        group.MapGet("/", async (string? search, bool? critical, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(search, critical ?? false, ct)));

        group.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } product
                ? Results.Ok(product)
                : Results.NotFound());

        group.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            var product = await service.CreateAsync(request, ct);
            return Results.Created($"/api/products/{product.Id}", product);
        });

        group.MapPut("/{id:guid}/critical-level", async (Guid id, UpdateCriticalLevelRequest request, IProductService service, CancellationToken ct) =>
            await service.UpdateCriticalLevelAsync(id, request, ct) is { } product
                ? Results.Ok(product)
                : Results.NotFound());

        // The ledger of one product, newest first
        group.MapGet("/{id:guid}/movements", async (Guid id, IProductService service, CancellationToken ct) =>
            await service.GetMovementsAsync(id, ct) is { } movements
                ? Results.Ok(movements)
                : Results.NotFound());

        // Stock in / stock out: { "type": "Out", "quantity": 3, "note": "Perakende satış" }
        group.MapPost("/{id:guid}/movements", async (Guid id, CreateMovementRequest request, IProductService service, CancellationToken ct) =>
            await service.RecordMovementAsync(id, request, ct) is { } movement
                ? Results.Created($"/api/products/{id}/movements/{movement.Id}", movement)
                : Results.NotFound());

        return app;
    }
}
