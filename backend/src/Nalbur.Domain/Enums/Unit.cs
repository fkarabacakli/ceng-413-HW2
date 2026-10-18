namespace Nalbur.Domain.Enums;

/// <summary>How a product is counted on the shelf.</summary>
public enum Unit
{
    Adet,   // piece
    Paket,  // box / pack
    Kg,
    Metre,
    Litre,
}

public static class UnitRules
{
    /// <summary>You cannot sell half a drill bit or half a box of screws.</summary>
    public static bool IsWholeNumberOnly(this Unit unit) => unit is Unit.Adet or Unit.Paket;

    public static string Symbol(this Unit unit) => unit switch
    {
        Unit.Adet => "adet",
        Unit.Paket => "paket",
        Unit.Kg => "kg",
        Unit.Metre => "m",
        Unit.Litre => "lt",
        _ => unit.ToString(),
    };
}
