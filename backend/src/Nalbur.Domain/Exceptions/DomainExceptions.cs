using Nalbur.Domain.Enums;

namespace Nalbur.Domain.Exceptions;

/// <summary>A business rule was broken. Mapped to 400 Bad Request by the API.</summary>
public class DomainException(string message) : Exception(message);

/// <summary>Trying to take more out of stock than there is. Mapped to 409 Conflict.</summary>
public sealed class InsufficientStockException(string sku, decimal available, decimal requested, Unit unit)
    : DomainException($"Stok yetersiz: {sku} için mevcut {available:0.##} {unit.Symbol()}, istenen {requested:0.##} {unit.Symbol()}.")
{
    public decimal Available { get; } = available;
    public decimal Requested { get; } = requested;
}

/// <summary>Another product already uses this stock code. Mapped to 409 Conflict.</summary>
public sealed class DuplicateSkuException(string sku)
    : DomainException($"'{sku}' stok kodu zaten kullanılıyor.");
