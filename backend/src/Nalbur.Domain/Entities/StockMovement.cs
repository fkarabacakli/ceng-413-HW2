using Nalbur.Domain.Enums;

namespace Nalbur.Domain.Entities;

/// <summary>
/// One line in the stock ledger. Movements are never edited or deleted;
/// a mistake is corrected with a new opposite movement, like a real ledger.
/// </summary>
public sealed class StockMovement
{
    public Guid Id { get; }
    public Guid ProductId { get; }
    public MovementType Type { get; }
    public decimal Quantity { get; }
    public decimal BalanceAfter { get; }
    public string? Note { get; }
    public DateTime CreatedAtUtc { get; }

    internal StockMovement(Guid productId, MovementType type, decimal quantity, decimal balanceAfter, string? note, DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        BalanceAfter = balanceAfter;
        Note = note;
        CreatedAtUtc = createdAtUtc;
    }
}
