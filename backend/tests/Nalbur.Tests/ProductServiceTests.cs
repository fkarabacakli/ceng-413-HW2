using Nalbur.Application;
using Nalbur.Application.Dtos;
using Nalbur.Application.Services;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Exceptions;
using Nalbur.Domain.Repositories;

namespace Nalbur.Tests;

/// <summary>
/// The Application layer runs against a fake repository and a fixed clock:
/// no database, no web server, no Docker. This is what the onion structure buys us.
/// </summary>
public class ProductServiceTests
{
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero));
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(new FakeProductRepository(), new StockSettings(DefaultCriticalLevel: 2.5m), _clock);
    }

    [Fact]
    public async Task Create_uses_the_default_critical_level_from_settings()
    {
        var product = await _service.CreateAsync(new CreateProductRequest("TEL-2", "Galvaniz tel", Unit.Kg, null, null));

        Assert.Equal(2.5m, product.CriticalLevel);
    }

    [Fact]
    public async Task Default_critical_level_is_rounded_up_for_countable_units()
    {
        var product = await _service.CreateAsync(new CreateProductRequest("MTK-6", "Matkap ucu", Unit.Adet, null, null));

        Assert.Equal(3m, product.CriticalLevel);
    }

    [Fact]
    public async Task Initial_stock_becomes_the_first_ledger_line()
    {
        var product = await _service.CreateAsync(new CreateProductRequest("MTK-6", "Matkap ucu", Unit.Adet, 5, 40));

        var ledger = await _service.GetMovementsAsync(product.Id);

        Assert.Equal(40m, product.Stock);
        Assert.NotNull(ledger);
        var line = Assert.Single(ledger);
        Assert.Equal(MovementType.In, line.Type);
    }

    [Fact]
    public async Task Duplicate_sku_is_rejected_case_insensitively()
    {
        await _service.CreateAsync(new CreateProductRequest("DBL-8", "Dübel", Unit.Paket, 4, null));

        await Assert.ThrowsAsync<DuplicateSkuException>(() =>
            _service.CreateAsync(new CreateProductRequest("dbl-8", "Başka dübel", Unit.Paket, 4, null)));
    }

    [Fact]
    public async Task Selling_more_than_available_is_rejected()
    {
        var product = await _service.CreateAsync(new CreateProductRequest("SLK-280", "Silikon", Unit.Adet, 6, 3));

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            _service.RecordMovementAsync(product.Id, new CreateMovementRequest(MovementType.Out, 5, "Satış")));

        var after = await _service.GetByIdAsync(product.Id);
        Assert.Equal(3m, after!.Stock);
    }

    [Fact]
    public async Task Movement_for_unknown_product_returns_null()
    {
        var result = await _service.RecordMovementAsync(Guid.NewGuid(), new CreateMovementRequest(MovementType.In, 1, null));

        Assert.Null(result);
    }

    [Fact]
    public async Task Critical_products_are_listed_first()
    {
        await _service.CreateAsync(new CreateProductRequest("A-1", "Ankraj", Unit.Adet, 2, 50));
        await _service.CreateAsync(new CreateProductRequest("Z-1", "Zımpara", Unit.Adet, 20, 5));

        var list = await _service.GetAllAsync(search: null, criticalOnly: false);

        Assert.Equal("Z-1", list[0].Sku);
        Assert.True(list[0].IsCritical);
    }

    [Fact]
    public async Task Summary_counts_only_the_last_24_hours()
    {
        var product = await _service.CreateAsync(new CreateProductRequest("PNS-1", "Pense", Unit.Adet, 1, 10));
        _clock.Advance(TimeSpan.FromHours(30));
        await _service.RecordMovementAsync(product.Id, new CreateMovementRequest(MovementType.Out, 1, null));

        var summary = await _service.GetSummaryAsync();

        Assert.Equal(1, summary.MovementsLast24h); // the opening stock is 30 hours old
    }

    private sealed class FixedClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now = _now.Add(by);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _items = new();

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Product>>(_items.Values.ToList());

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_items.GetValueOrDefault(id));

        public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(p => string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(Product product, CancellationToken ct = default)
        {
            _items[product.Id] = product;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product, CancellationToken ct = default) => Task.CompletedTask;
    }
}
