using Nalbur.Application.Dtos;

namespace Nalbur.Application.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(string? search, bool criticalOnly, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> UpdateCriticalLevelAsync(Guid id, UpdateCriticalLevelRequest request, CancellationToken ct = default);

    /// <summary>Returns null when the product does not exist.</summary>
    Task<MovementDto?> RecordMovementAsync(Guid productId, CreateMovementRequest request, CancellationToken ct = default);

    /// <summary>Newest first. Returns null when the product does not exist.</summary>
    Task<IReadOnlyList<MovementDto>?> GetMovementsAsync(Guid productId, CancellationToken ct = default);

    Task<IReadOnlyList<MovementDto>> GetRecentMovementsAsync(int take, CancellationToken ct = default);
    Task<StockSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}
