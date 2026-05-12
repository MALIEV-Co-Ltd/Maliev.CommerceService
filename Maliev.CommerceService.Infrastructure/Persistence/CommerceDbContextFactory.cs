using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.CommerceService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public sealed class CommerceDbContextFactory : IDesignTimeDbContextFactory<CommerceDbContext>
{
    /// <inheritdoc />
    public CommerceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=maliev_commerce_design;Username=postgres;Password=postgres")
            .Options;

        return new CommerceDbContext(options);
    }
}
