using System.Text.RegularExpressions;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Exceptions;

namespace Nalbur.Domain.Entities;

/// <summary>
/// Aggregate root. The stock level can only change through Receive / Issue,
/// so every change leaves a ledger line and the rules are checked in one place.
/// </summary>
public sealed partial class Product
{
    public const int MaxNameLength = 120;
    public const int MaxSkuLength = 32;
    public const int MaxNoteLength = 200;

    private readonly List<StockMovement> _movements = [];

    public Guid Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public Unit Unit { get; }
    public decimal Stock { get; private set; }
    public decimal CriticalLevel { get; private set; }
    public DateTime CreatedAtUtc { get; }

    public IReadOnlyList<StockMovement> Movements => _movements;
    public bool IsCritical => Stock <= CriticalLevel;

    private Product(string sku, string name, Unit unit, decimal criticalLevel, DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        Sku = sku;
        Name = name;
        Unit = unit;
        CriticalLevel = criticalLevel;
        CreatedAtUtc = createdAtUtc;
    }

    public static Product Create(string? sku, string? name, Unit unit, decimal criticalLevel, DateTime nowUtc)
    {
        var normalizedSku = NormalizeSku(sku);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Ürün adı zorunludur.");
        }

        var trimmedName = name.Trim();
        if (trimmedName.Length > MaxNameLength)
        {
            throw new DomainException($"Ürün adı en fazla {MaxNameLength} karakter olabilir.");
        }

        if (!Enum.IsDefined(unit))
        {
            throw new DomainException("Geçersiz birim.");
        }

        EnsureValidCriticalLevel(criticalLevel, unit);

        return new Product(normalizedSku, trimmedName, unit, criticalLevel, nowUtc);
    }

    /// <summary>Stok girişi: goods arrive from a supplier.</summary>
    public StockMovement Receive(decimal quantity, string? note, DateTime nowUtc)
    {
        EnsureValidQuantity(quantity);
        Stock += quantity;
        return Record(MovementType.In, quantity, note, nowUtc);
    }

    /// <summary>Stok çıkışı: goods are sold. Stock can never go below zero.</summary>
    public StockMovement Issue(decimal quantity, string? note, DateTime nowUtc)
    {
        EnsureValidQuantity(quantity);
        if (quantity > Stock)
        {
            throw new InsufficientStockException(Sku, Stock, quantity, Unit);
        }

        Stock -= quantity;
        return Record(MovementType.Out, quantity, note, nowUtc);
    }

    public void ChangeCriticalLevel(decimal criticalLevel)
    {
        EnsureValidCriticalLevel(criticalLevel, Unit);
        CriticalLevel = criticalLevel;
    }

    private StockMovement Record(MovementType type, decimal quantity, string? note, DateTime nowUtc)
    {
        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (trimmedNote is { Length: > MaxNoteLength })
        {
            throw new DomainException($"Not en fazla {MaxNoteLength} karakter olabilir.");
        }

        var movement = new StockMovement(Id, type, quantity, Stock, trimmedNote, nowUtc);
        _movements.Add(movement);
        return movement;
    }

    private void EnsureValidQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Miktar sıfırdan büyük olmalıdır.");
        }

        if (Unit.IsWholeNumberOnly() && quantity != decimal.Truncate(quantity))
        {
            throw new DomainException($"'{Unit.Symbol()}' biriminde küsuratlı miktar girilemez.");
        }
    }

    private static void EnsureValidCriticalLevel(decimal criticalLevel, Unit unit)
    {
        if (criticalLevel < 0)
        {
            throw new DomainException("Kritik seviye negatif olamaz.");
        }

        if (unit.IsWholeNumberOnly() && criticalLevel != decimal.Truncate(criticalLevel))
        {
            throw new DomainException($"'{unit.Symbol()}' biriminde kritik seviye tam sayı olmalıdır.");
        }
    }

    private static string NormalizeSku(string? sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("Stok kodu zorunludur.");
        }

        var normalized = sku.Trim().ToUpperInvariant();
        if (normalized.Length > MaxSkuLength || !SkuPattern().IsMatch(normalized))
        {
            throw new DomainException($"Stok kodu yalnızca harf, rakam ve '-' içerebilir (en fazla {MaxSkuLength} karakter).");
        }

        return normalized;
    }

    [GeneratedRegex("^[A-Z0-9][A-Z0-9-]*$")]
    private static partial Regex SkuPattern();
}
