using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Domain.Entities;
using Maliev.CommerceService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using CommerceApplicationService = Maliev.CommerceService.Application.Services.CommerceService;

namespace Maliev.CommerceService.Tests.Integration;

public sealed class CommerceRepositoryTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task UpdateProductAsync_WhenReplacingListingDetails_PersistsUpdatedProduct()
    {
        await using var setupDbContext = fixture.CreateDbContext();
        await setupDbContext.Database.EnsureCreatedAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Handle = $"e2e-test-collection-{suffix}",
            Title = "E2E Test Collection",
            IsPublished = true
        };
        setupDbContext.Collections.Add(collection);
        await setupDbContext.SaveChangesAsync();

        var createService = new CommerceApplicationService(new CommerceRepository(setupDbContext));
        var created = await createService.CreateProductAsync(new CreateProductRequest
        {
            Handle = $"e2e-test-product-{suffix}",
            Title = "E2E Test Product",
            Brand = "MALIEV",
            Summary = "Initial summary",
            Description = "Initial description",
            ProductType = "Machine",
            Status = ProductStatus.Draft,
            CollectionHandles = [collection.Handle],
            Variants =
            [
                new CreateProductVariantRequest
                {
                    Sku = $"E2E-SKU-{suffix}",
                    Title = "Default",
                    PriceAmount = 12345.67m,
                    Currency = "THB",
                    InventoryQuantity = 7
                }
            ],
            Media =
            [
                new CreateProductMediaRequest
                {
                    Url = "https://example.com/original.png",
                    AltText = "Original image",
                    SortOrder = 0
                }
            ]
        }, CancellationToken.None);

        await using var updateDbContext = fixture.CreateDbContext();
        var updateService = new CommerceApplicationService(new CommerceRepository(updateDbContext));

        var updated = await updateService.UpdateProductAsync(created.Id, new UpdateProductRequest
        {
            Handle = created.Handle,
            Title = "E2E Test Product Updated",
            Brand = "MALIEV",
            Summary = "Updated summary",
            Description = "Updated description",
            ProductType = "Machine",
            Status = ProductStatus.Published,
            CollectionHandles = [collection.Handle],
            Variants =
            [
                new CreateProductVariantRequest
                {
                    Sku = $"E2E-SKU-{suffix}",
                    Title = "Default",
                    PriceAmount = 23456.78m,
                    Currency = "THB",
                    InventoryQuantity = 9
                }
            ],
            Media =
            [
                new CreateProductMediaRequest
                {
                    Url = "https://example.com/updated.png",
                    AltText = "Updated image",
                    SortOrder = 0
                }
            ]
        }, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("E2E Test Product Updated", updated.Title);
        Assert.Equal(ProductStatus.Published, updated.Status);
        Assert.Equal(23456.78m, updated.Variants.Single().PriceAmount);
        Assert.Equal(9, updated.Variants.Single().InventoryQuantity);
        Assert.Equal("https://example.com/updated.png", updated.Media.Single().Url);
        Assert.Equal(collection.Handle, updated.Collections.Single().Handle);
    }

    [Fact]
    public async Task Repository_PersistsProductCartCheckoutAndStoreOrder()
    {
        await using var dbContext = fixture.CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        var repository = new CommerceRepository(dbContext);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Handle = "pneumatic-injection-molding-machine",
            Title = "Pneumatic Injection Molding Machine",
            Summary = "Compact workshop molding machine",
            ProductType = "Machine",
            Status = ProductStatus.Published
        };
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Sku = "PIMM-001",
            Title = "Standard",
            PriceAmount = 45000,
            Currency = "THB",
            InventoryQuantity = 2
        };
        product.Variants.Add(variant);
        product.Media.Add(new ProductMedia
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Url = "https://example.com/pimm.jpg",
            AltText = "Pneumatic injection molding machine"
        });

        await repository.AddProductAsync(product, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var (products, totalCount) = await repository.ListProductsAsync("pneumatic", null, 1, 10, includeDrafts: false, CancellationToken.None);
        Assert.Equal(1, totalCount);
        Assert.Single(products);
        Assert.Single(products[0].Variants);

        var customerId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), CustomerId = customerId, Currency = "THB" };
        cart.Lines.Add(new CartLine
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductVariantId = variant.Id,
            Quantity = 1,
            UnitPriceAmount = variant.PriceAmount,
            Currency = variant.Currency,
            Sku = variant.Sku,
            Title = product.Title
        });
        await repository.AddCartAsync(cart, CancellationToken.None);

        var checkout = new CheckoutSession
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            CustomerId = customerId,
            TotalAmount = 45000,
            Currency = "THB"
        };
        await repository.AddCheckoutSessionAsync(checkout, CancellationToken.None);

        var order = new StoreOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "MWS-TEST-001",
            CustomerId = customerId,
            CheckoutSessionId = checkout.Id,
            TotalAmount = 45000,
            Currency = "THB"
        };
        order.Lines.Add(new StoreOrderLine
        {
            Id = Guid.NewGuid(),
            StoreOrderId = order.Id,
            ProductVariantId = variant.Id,
            Quantity = 1,
            UnitPriceAmount = 45000,
            Currency = "THB",
            Sku = "PIMM-001",
            Title = "Pneumatic Injection Molding Machine"
        });
        await repository.AddStoreOrderAsync(order, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var orders = await repository.ListStoreOrdersAsync(customerId, CancellationToken.None);

        Assert.Single(orders);
        Assert.Equal("MWS-TEST-001", orders[0].OrderNumber);
        Assert.Single(orders[0].Lines);
    }
}
