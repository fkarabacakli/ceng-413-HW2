using Nalbur.Application.Dtos;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Exceptions;
using Nalbur.Domain.Repositories;

namespace Nalbur.Application.Services;

public sealed class ProductService(
    IProductRepository repository,
    StockSettings settings,
    TimeProvider clock) : IProductService
{
    // Two cashiers selling the last box at the same moment must not both succeed.
    // One lock is enough for a single in-memory instance; with a real database
    // this would be a transaction with a row lock or an optimistic concurrency check.
    private static readonly SemaphoreSlim WriteLock = new(1, 1);

    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(string? search, bool criticalOnly, CancellationToken ct = default)
    {
        var products = await repository.GetAllAsync(ct);
        IEnumerable<Product> query = products;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Sku.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (criticalOnly)
        {
            query = query.Where(p => p.IsCritical);
        }

        return query
            .OrderByDescending(p => p.IsCritical)   // problems first
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ProductDto.From)
            .ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        return product is null ? null : ProductDto.From(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        if (request.Unit is not { } unit)
        {
            throw new DomainException("Birim zorunludur (Adet, Paket, Kg, Metre, Litre).");
        }

        var criticalLevel = request.CriticalLevel ?? DefaultCriticalFor(unit);

        await WriteLock.WaitAsync(ct);
        try
        {
            var product = Product.Create(request.Sku, request.Name, unit, criticalLevel, UtcNow);

            if (await repository.GetBySkuAsync(product.Sku, ct) is not null)
            {
                throw new DuplicateSkuException(product.Sku);
            }

            if (request.InitialStock is { } initial && initial > 0)
            {
                product.Receive(initial, "Açılış stoğu", UtcNow);
            }

            await repository.AddAsync(product, ct);
            return ProductDto.From(product);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    public async Task<ProductDto?> UpdateCriticalLevelAsync(Guid id, UpdateCriticalLevelRequest request, CancellationToken ct = default)
    {
        if (request.CriticalLevel is not { } level)
        {
            throw new DomainException("Kritik seviye zorunludur.");
        }

        await WriteLock.WaitAsync(ct);
        try
        {
            var product = await repository.GetByIdAsync(id, ct);
            if (product is null)
            {
                return null;
            }

            product.ChangeCriticalLevel(level);
            await repository.UpdateAsync(product, ct);
            return ProductDto.From(product);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    public async Task<MovementDto?> RecordMovementAsync(Guid productId, CreateMovementRequest request, CancellationToken ct = default)
    {
        if (request.Type is not { } type)
        {
            throw new DomainException("Hareket tipi zorunludur (In veya Out).");
        }

        if (request.Quantity is not { } quantity)
        {
            throw new DomainException("Miktar zorunludur.");
        }

        await WriteLock.WaitAsync(ct);
        try
        {
            var product = await repository.GetByIdAsync(productId, ct);
            if (product is null)
            {
                return null;
            }

            var movement = type == MovementType.In
                ? product.Receive(quantity, request.Note, UtcNow)
                : product.Issue(quantity, request.Note, UtcNow);

            await repository.UpdateAsync(product, ct);
            return MovementDto.From(product, movement);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    public async Task<IReadOnlyList<MovementDto>?> GetMovementsAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(productId, ct);
        if (product is null)
        {
            return null;
        }

        return product.Movements
            .OrderByDescending(m => m.CreatedAtUtc)
            .Select(m => MovementDto.From(product, m))
            .ToList();
    }

    public async Task<IReadOnlyList<MovementDto>> GetRecentMovementsAsync(int take, CancellationToken ct = default)
    {
        var products = await repository.GetAllAsync(ct);
        return products
            .SelectMany(p => p.Movements.Select(m => MovementDto.From(p, m)))
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(Math.Clamp(take, 1, 100))
            .ToList();
    }

    public async Task<StockSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var products = await repository.GetAllAsync(ct);
        var since = UtcNow.AddHours(-24);

        return new StockSummaryDto(
            ProductCount: products.Count,
            CriticalCount: products.Count(p => p.IsCritical),
            OutOfStockCount: products.Count(p => p.Stock == 0),
            MovementsLast24h: products.Sum(p => p.Movements.Count(m => m.CreatedAtUtc >= since)),
            DefaultCriticalLevel: settings.DefaultCriticalLevel);
    }

    // The default from the environment may be fractional (e.g. 2.5); for
    // whole-number units round it up so the product can still be created.
    private decimal DefaultCriticalFor(Unit unit) =>
        unit.IsWholeNumberOnly()
            ? decimal.Ceiling(settings.DefaultCriticalLevel)
            : settings.DefaultCriticalLevel;
}
