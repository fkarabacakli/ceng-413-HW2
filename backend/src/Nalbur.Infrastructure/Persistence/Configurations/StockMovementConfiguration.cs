using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nalbur.Domain.Entities;

namespace Nalbur.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        // Set by the Domain; with a generated key EF would treat a new line as an UPDATE.
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(8);
        builder.Property(m => m.Quantity).HasPrecision(14, 3);
        builder.Property(m => m.BalanceAfter).HasPrecision(14, 3);
        builder.Property(m => m.Note).HasMaxLength(Product.MaxNoteLength);
        builder.Property(m => m.CreatedAtUtc);

        builder.HasIndex(m => m.CreatedAtUtc); // "latest movements" and the 24 h summary
    }
}
