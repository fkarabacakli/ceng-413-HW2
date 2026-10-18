using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;

namespace Nalbur.Application.Dtos;

// What the outside world sees. Entities never leave the core.

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    Unit Unit,
    string UnitSymbol,
    decimal Stock,
    decimal CriticalLevel,
    bool IsCritical,
    DateTime? LastMovementAtUtc)
{
    public static ProductDto From(Product p) => new(
        p.Id, p.Sku, p.Name, p.Unit, p.Unit.Symbol(), p.Stock, p.CriticalLevel, p.IsCritical,
        p.Movements.Count == 0 ? null : p.Movements[^1].CreatedAtUtc);
}

public sealed record MovementDto(
    Guid Id,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    MovementType Type,
    decimal Quantity,
    decimal BalanceAfter,
    string UnitSymbol,
    string? Note,
    DateTime CreatedAtUtc)
{
    public static MovementDto From(Product p, StockMovement m) => new(
        m.Id, p.Id, p.Sku, p.Name, m.Type, m.Quantity, m.BalanceAfter, p.Unit.Symbol(), m.Note, m.CreatedAtUtc);
}

public sealed record StockSummaryDto(
    int ProductCount,
    int CriticalCount,
    int OutOfStockCount,
    int MovementsLast24h,
    decimal DefaultCriticalLevel);

// Requests: every field is nullable so a missing field becomes a clear
// validation message instead of a silent default value.

public sealed record CreateProductRequest(
    string? Sku,
    string? Name,
    Unit? Unit,
    decimal? CriticalLevel,
    decimal? InitialStock);

public sealed record CreateMovementRequest(MovementType? Type, decimal? Quantity, string? Note);

public sealed record UpdateCriticalLevelRequest(decimal? CriticalLevel);
