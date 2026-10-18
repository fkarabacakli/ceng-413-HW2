using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Repositories;

namespace Nalbur.Infrastructure.Persistence;

/// <summary>
/// Stores products and their ledgers in PostgreSQL. A product is always loaded with
/// its movements, because the aggregate needs them to check the stock rules.
/// </summary>
public sealed class EfProductRepository(NalburDbContext db) : IProductRepository
{
    private IQueryable<Product> ProductsWithLedger => db.Products.Include(p => p.Movements);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await ProductsWithLedger.AsSplitQuery().ToListAsync(ct);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ProductsWithLedger.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        // Product.Create stores stock codes in upper case, so a plain comparison is enough.
        var normalized = sku.Trim().ToUpperInvariant();
        return ProductsWithLedger.FirstOrDefaultAsync(p => p.Sku == normalized, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        // The product was loaded by this context, so change tracking already sees the
        // new stock level and the new ledger line; saving writes both in one transaction.
        await db.SaveChangesAsync(ct);
    }
}
