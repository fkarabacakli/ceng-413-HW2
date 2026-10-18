using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nalbur.Domain.Entities;

namespace Nalbur.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        // The Domain creates the Guid itself, so the database must not generate one.
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Sku).HasMaxLength(Product.MaxSkuLength).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique(); // backs up the unique stock code rule

        builder.Property(p => p.Name).HasMaxLength(Product.MaxNameLength).IsRequired();
        builder.Property(p => p.Unit).HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.Stock).HasPrecision(14, 3);
        builder.Property(p => p.CriticalLevel).HasPrecision(14, 3);
        builder.Property(p => p.CreatedAtUtc);

        builder.Ignore(p => p.IsCritical); // computed from Stock and CriticalLevel

        // The ledger is read and written through the private _movements list.
        builder.HasMany(p => p.Movements)
            .WithOne()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(p => p.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
