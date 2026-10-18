using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nalbur.Infrastructure.Persistence;

/// <summary>
/// Used only by the `dotnet ef` tool to create migrations. Creating a migration does not
/// connect to the database, so the fallback has no credentials; this class is compiled
/// into the image, so it must never contain a password.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NalburDbContext>
{
    public NalburDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Nalbur")
            ?? "Host=localhost;Database=nalbur";
        var options = new DbContextOptionsBuilder<NalburDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new NalburDbContext(options);
    }
}
