using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Interfaces;
using Maliev.CommerceService.Domain.Entities;
using Maliev.CommerceService.Infrastructure.Shopify;

namespace Maliev.CommerceService.Tests.Unit;

public sealed class ShopifyCatalogImportServiceTests
{
    [Fact]
    public async Task ImportAsync_WithShopifyProduct_CreatesCatalogProduct()
    {
        var repository = new FakeCommerceRepository();
        var service = new ShopifyCatalogImportService(new FakeShopifyAdminClient(), repository);

        var result = await service.ImportAsync(new ShopifyCatalogImportRequest { PageSize = 10 }, CancellationToken.None);

        Assert.Equal(1, result.ProductsRead);
        Assert.Equal(1, result.ProductsCreated);
        Assert.Equal(1, result.CollectionsCreated);
        Assert.False(result.DryRun);
        Assert.True(repository.SaveCalled);
        var product = Assert.Single(repository.Products);
        Assert.Equal("simmount-base", product.Handle);
        Assert.Equal(ProductStatus.Published, product.Status);
        Assert.Equal("MALIEV", product.Brand);
        Assert.Equal("THB", Assert.Single(product.Variants).Currency);
        Assert.Equal("""{"Color":"Black"}""", product.Variants[0].OptionValuesJson);
        Assert.Equal("https://cdn.shopify.com/product.png", Assert.Single(product.Media).Url);
        Assert.Single(product.ProductCollections);
    }

    [Fact]
    public async Task ImportAsync_WithDryRun_DoesNotPersistChanges()
    {
        var repository = new FakeCommerceRepository();
        var service = new ShopifyCatalogImportService(new FakeShopifyAdminClient(), repository);

        var result = await service.ImportAsync(new ShopifyCatalogImportRequest { DryRun = true }, CancellationToken.None);

        Assert.True(result.DryRun);
        Assert.Equal(1, result.ProductsCreated);
        Assert.Empty(repository.Products);
        Assert.Empty(repository.Collections);
        Assert.False(repository.SaveCalled);
    }

    [Fact]
    public async Task ImportAsync_WithInjectionMachineFilter_SkipsNonMatchingProducts()
    {
        var repository = new FakeCommerceRepository();
        var shopifyClient = new FakeShopifyAdminClient(includeInjectionMachineDataset: true);
        var service = new ShopifyCatalogImportService(shopifyClient, repository);

        var result = await service.ImportAsync(new ShopifyCatalogImportRequest
        {
            SearchQuery = "injection molding machine",
            Keywords = ["injection molding"],
            EnsureCollectionHandle = "injection-molding-machines",
            EnsureCollectionTitle = "Injection Molding Machines"
        }, CancellationToken.None);

        Assert.Equal("injection molding machine", shopifyClient.LastQuery);
        Assert.Equal(2, result.ProductsRead);
        Assert.Equal(1, result.ProductsCreated);
        Assert.Equal(1, result.ProductsSkipped);
        Assert.Equal("pneumatic-injection-molding-machine", Assert.Single(repository.Products).Handle);
        Assert.Contains(repository.Collections, collection => collection.Handle == "injection-molding-machines");
    }

    private sealed class FakeShopifyAdminClient(bool includeInjectionMachineDataset = false) : IShopifyAdminClient
    {
        private readonly bool _includeInjectionMachineDataset = includeInjectionMachineDataset;

        public string? LastQuery { get; private set; }

        public Task<ShopifyCatalogPage> GetCatalogPageAsync(int first, string? after, string? query, CancellationToken cancellationToken)
        {
            LastQuery = query;
            var page = new ShopifyCatalogPage
            {
                Shop = new ShopifyShopNode { CurrencyCode = "THB" },
                Products = new ShopifyProductConnection
                {
                    PageInfo = new ShopifyPageInfo { HasNextPage = false },
                    Nodes =
                    [
                        new ShopifyProductNode
                        {
                            Id = "gid://shopify/Product/1",
                            Handle = "simmount-base",
                            Title = "SimMount Base",
                            Vendor = "MALIEV",
                            ProductType = "SimMount",
                            DescriptionHtml = "<p>Mounting fixture for production benches.</p>",
                            Status = "ACTIVE",
                            Tags = ["fixture"],
                            Variants = new ShopifyVariantConnection
                            {
                                Nodes =
                                [
                                    new ShopifyVariantNode
                                    {
                                        Id = "gid://shopify/ProductVariant/10",
                                        Title = "Black",
                                        Sku = "SIM-BASE-BLK",
                                        Price = "1250.00",
                                        InventoryQuantity = 12,
                                        SelectedOptions = [new ShopifySelectedOptionNode { Name = "Color", Value = "Black" }]
                                    }
                                ]
                            },
                            Media = new ShopifyMediaConnection
                            {
                                Nodes =
                                [
                                    new ShopifyMediaNode
                                    {
                                        Alt = "SimMount Base",
                                        MediaContentType = "IMAGE",
                                        Preview = new ShopifyMediaPreview
                                        {
                                            Image = new ShopifyImageNode { Url = "https://cdn.shopify.com/product.png" }
                                        }
                                    }
                                ]
                            },
                            Collections = new ShopifyCollectionConnection
                            {
                                Nodes =
                                [
                                    new ShopifyCollectionNode
                                    {
                                        Id = "gid://shopify/Collection/100",
                                        Handle = "simmount",
                                        Title = "SimMount",
                                        Description = "SimMount products"
                                    }
                                ]
                            }
                        }
                    ]
                }
            };

            if (_includeInjectionMachineDataset)
            {
                page.Products.Nodes.Clear();
                page.Products.Nodes.Add(new ShopifyProductNode
                {
                    Id = "gid://shopify/Product/2",
                    Handle = "pneumatic-injection-molding-machine",
                    Title = "Pneumatic Injection Molding Machine",
                    Vendor = "MALIEV",
                    ProductType = "Machine",
                    DescriptionHtml = "<p>Bench-top pneumatic injection molding machine.</p>",
                    Status = "ACTIVE",
                    Tags = ["injection molding", "machine"],
                    Variants = new ShopifyVariantConnection
                    {
                        Nodes =
                        [
                            new ShopifyVariantNode
                            {
                                Id = "gid://shopify/ProductVariant/20",
                                Title = "Standard",
                                Sku = "PIMM-001",
                                Price = "49000.00",
                                InventoryQuantity = 2
                            }
                        ]
                    }
                });
                page.Products.Nodes.Add(new ShopifyProductNode
                {
                    Id = "gid://shopify/Product/3",
                    Handle = "simmount-plate",
                    Title = "SimMount Plate",
                    Vendor = "MALIEV",
                    ProductType = "SimMount",
                    DescriptionHtml = "<p>Fixture plate.</p>",
                    Status = "ACTIVE",
                    Tags = ["fixture"]
                });
            }

            return Task.FromResult(page);
        }
    }

    private sealed class FakeCommerceRepository : ICommerceRepository
    {
        public List<Product> Products { get; } = [];

        public List<Collection> Collections { get; } = [];

        public bool SaveCalled { get; private set; }

        public Task<(IReadOnlyList<Product> Items, int TotalCount)> ListProductsAsync(string? query, string? collectionHandle, int page, int pageSize, bool includeDrafts, CancellationToken cancellationToken)
        {
            return Task.FromResult(((IReadOnlyList<Product>)Products, Products.Count));
        }

        public Task<Product?> GetProductByHandleAsync(string handle, CancellationToken cancellationToken)
        {
            return Task.FromResult(Products.FirstOrDefault(product => product.Handle == handle));
        }

        public Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Products.FirstOrDefault(product => product.Id == id));
        }

        public Task AddProductAsync(Product product, CancellationToken cancellationToken)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Collection>> ListCollectionsAsync(bool includeUnpublished, CancellationToken cancellationToken)
        {
            return Task.FromResult((IReadOnlyList<Collection>)Collections);
        }

        public Task<Collection?> GetCollectionByHandleAsync(string handle, CancellationToken cancellationToken)
        {
            return Task.FromResult(Collections.FirstOrDefault(collection => collection.Handle == handle));
        }

        public Task<Collection?> GetCollectionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Collections.FirstOrDefault(collection => collection.Id == id));
        }

        public Task AddCollectionAsync(Collection collection, CancellationToken cancellationToken)
        {
            Collections.Add(collection);
            return Task.CompletedTask;
        }

        public Task<ProductVariant?> GetVariantAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Products.SelectMany(product => product.Variants).FirstOrDefault(variant => variant.Id == id));
        }

        public Task AddCartAsync(Cart cart, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Cart?> GetCartAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Cart?>(null);

        public Task AddCheckoutSessionAsync(CheckoutSession checkoutSession, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<CheckoutSession?> GetCheckoutSessionAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<CheckoutSession?>(null);

        public Task AddStoreOrderAsync(StoreOrder order, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<StoreOrder>> ListStoreOrdersAsync(Guid? customerId, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<StoreOrder>)[]);

        public Task<StoreOrder?> GetStoreOrderAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<StoreOrder?>(null);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCalled = true;
            return Task.CompletedTask;
        }
    }
}
