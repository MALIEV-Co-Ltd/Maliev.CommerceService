using Maliev.CommerceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Maliev.CommerceService.Tests.Integration;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public CommerceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        return new CommerceDbContext(options);
    }
}
