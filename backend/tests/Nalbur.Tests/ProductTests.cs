using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Exceptions;

namespace Nalbur.Tests;

/// <summary>Business rules of the Product aggregate. Pure C#, nothing to mock.</summary>
public class ProductTests
{
    private static readonly DateTime Now = new(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);

    private static Product NewProduct(Unit unit = Unit.Adet, decimal critical = 5) =>
        Product.Create("mtk-6", "Matkap ucu 6 mm", unit, critical, Now);

    [Fact]
    public void Sku_is_trimmed_and_upper_cased()
    {
        var product = Product.Create("  vd-4x40 ", "Vida", Unit.Paket, 2, Now);

        Assert.Equal("VD-4X40", product.Sku);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("has space")]
    [InlineData("-STARTS-WITH-DASH")]
    public void Invalid_sku_is_rejected(string? sku)
    {
        Assert.Throws<DomainException>(() => Product.Create(sku, "Vida", Unit.Adet, 1, Now));
    }

    [Fact]
    public void Receive_adds_stock_and_writes_a_ledger_line()
    {
        var product = NewProduct();

        var movement = product.Receive(20, "İrsaliye #1", Now);

        Assert.Equal(20m, product.Stock);
        Assert.Equal(MovementType.In, movement.Type);
        Assert.Equal(20m, movement.BalanceAfter);
        Assert.Single(product.Movements);
    }

    [Fact]
    public void Stock_can_never_go_below_zero()
    {
        var product = NewProduct();
        product.Receive(3, null, Now);

        var ex = Assert.Throws<InsufficientStockException>(() => product.Issue(4, null, Now));

        Assert.Equal(3m, ex.Available);
        Assert.Equal(3m, product.Stock);          // unchanged
        Assert.Single(product.Movements);        // no ledger line for the failed sale
    }

    [Fact]
    public void Issuing_exactly_the_whole_stock_is_allowed()
    {
        var product = NewProduct();
        product.Receive(3, null, Now);

        product.Issue(3, null, Now);

        Assert.Equal(0m, product.Stock);
        Assert.True(product.IsCritical);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantity_must_be_positive(int quantity)
    {
        var product = NewProduct();

        Assert.Throws<DomainException>(() => product.Receive(quantity, null, Now));
    }

    [Theory]
    [InlineData(Unit.Adet)]
    [InlineData(Unit.Paket)]
    public void Countable_units_reject_fractions(Unit unit)
    {
        var product = NewProduct(unit, critical: 1);

        Assert.Throws<DomainException>(() => product.Receive(1.5m, null, Now));
    }

    [Theory]
    [InlineData(Unit.Kg)]
    [InlineData(Unit.Metre)]
    [InlineData(Unit.Litre)]
    public void Measured_units_accept_fractions(Unit unit)
    {
        var product = NewProduct(unit, critical: 1);

        product.Receive(2.75m, null, Now);

        Assert.Equal(2.75m, product.Stock);
    }

    [Fact]
    public void Product_is_critical_at_or_below_its_level()
    {
        var product = NewProduct(critical: 5);
        product.Receive(6, null, Now);
        Assert.False(product.IsCritical);

        product.Issue(1, null, Now);

        Assert.True(product.IsCritical);
    }
}
