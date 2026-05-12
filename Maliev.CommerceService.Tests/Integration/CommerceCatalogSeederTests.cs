using Maliev.CommerceService.Domain.Entities;
using Maliev.CommerceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CommerceService.Tests.Integration;

public sealed class CommerceCatalogSeederTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task SeedStarterCatalogAsync_WithEmptyDatabase_CreatesDraftMachineListings()
    {
        await using var dbContext = fixture.CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        await CommerceCatalogSeeder.SeedStarterCatalogAsync(dbContext, CancellationToken.None);

        var collection = await dbContext.Collections
            .Include(item => item.ProductCollections)
            .SingleAsync(item => item.Handle == "injection-molding-machines");
        var products = await dbContext.Products
            .Include(item => item.Variants)
            .OrderBy(item => item.Handle)
            .ToListAsync();

        Assert.True(collection.IsPublished);
        Assert.Equal(2, collection.ProductCollections.Count);
        Assert.Equal(2, products.Count);
        Assert.All(products, product => Assert.Equal(ProductStatus.Draft, product.Status));
        Assert.Contains(products, product => product.Handle == "pneumatic-injection-molding-machine-30g" && product.Variants.Single().PriceAmount == 99000);
        Assert.Contains(products, product => product.Handle == "pneumatic-injection-molding-machine-50g" && product.Description.Contains("350 C", StringComparison.Ordinal));
        Assert.Contains(products, product => product.Handle == "pneumatic-injection-molding-machine-50g" && product.Description.Contains("steel", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(products, product => product.Handle == "pneumatic-injection-molding-machine-50g" && product.Description.Contains("1015 mm", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SeedStarterCatalogAsync_WithExistingProduct_DoesNotOverwriteEmployeeEdits()
    {
        await using var dbContext = fixture.CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        await CommerceCatalogSeeder.SeedStarterCatalogAsync(dbContext, CancellationToken.None);

        var product = await dbContext.Products.SingleAsync(item => item.Handle == "pneumatic-injection-molding-machine-30g");
        product.Title = "Employee edited title";
        await dbContext.SaveChangesAsync();

        await CommerceCatalogSeeder.SeedStarterCatalogAsync(dbContext, CancellationToken.None);

        var products = await dbContext.Products
            .Where(item => item.Handle.StartsWith("pneumatic-injection-molding-machine"))
            .OrderBy(item => item.Handle)
            .ToListAsync();
        var editedProduct = await dbContext.Products.SingleAsync(item => item.Handle == "pneumatic-injection-molding-machine-30g");

        Assert.Equal(2, products.Count);
        Assert.Equal("Employee edited title", editedProduct.Title);
    }
}
