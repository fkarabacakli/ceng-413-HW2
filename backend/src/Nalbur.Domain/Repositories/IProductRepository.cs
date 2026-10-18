using Nalbur.Domain.Entities;

namespace Nalbur.Domain.Repositories;

/// <summary>
/// The core says WHAT it needs from storage; Infrastructure decides HOW.
/// Dependencies point inward: Infrastructure references Domain, never the other way.
/// </summary>
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
}
