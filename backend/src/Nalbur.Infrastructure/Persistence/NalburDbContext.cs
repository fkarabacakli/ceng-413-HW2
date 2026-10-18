using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;

namespace Nalbur.Infrastructure.Persistence;

/// <summary>
/// Code first: the C# model is the source of truth and the tables come from migrations.
/// The mapping lives in Configurations/, so the Domain classes carry no EF attributes.
/// </summary>
public sealed class NalburDbContext(DbContextOptions<NalburDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NalburDbContext).Assembly);
}
