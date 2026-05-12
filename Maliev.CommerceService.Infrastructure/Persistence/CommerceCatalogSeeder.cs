using System.Text.Json;

using Maliev.CommerceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.CommerceService.Infrastructure.Persistence;

/// <summary>
/// Seeds starter storefront catalog records that employees can refine in Intranet.
/// </summary>
public static class CommerceCatalogSeeder
{
    private const string MachineCollectionHandle = "injection-molding-machines";

    /// <summary>
    /// Seeds starter catalog records in a scoped database context.
    /// </summary>
    public static async Task SeedStarterCatalogAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        await SeedStarterCatalogAsync(dbContext, cancellationToken);
    }

    /// <summary>
    /// Seeds starter catalog records in the provided database context.
    /// </summary>
    public static async Task SeedStarterCatalogAsync(CommerceDbContext dbContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var machineCollection = await dbContext.Collections
            .FirstOrDefaultAsync(collection => collection.Handle == MachineCollectionHandle, cancellationToken);

        if (machineCollection is null)
        {
            machineCollection = new Collection
            {
                Id = Guid.NewGuid(),
                Handle = MachineCollectionHandle,
                Title = "Injection molding machines",
                Description = "Starter collection for MALIEV pneumatic injection molding machines, mold planning, and machine accessories.",
                IsPublished = true
            };
            await dbContext.Collections.AddAsync(machineCollection, cancellationToken);
        }

        foreach (var seedProduct in BuildStarterProducts())
        {
            var product = await dbContext.Products
                .Include(existing => existing.ProductCollections)
                .FirstOrDefaultAsync(existing => existing.Handle == seedProduct.Handle, cancellationToken);

            if (product is null)
            {
                product = seedProduct;
                await dbContext.Products.AddAsync(product, cancellationToken);
            }

            if (product.ProductCollections.All(link => link.CollectionId != machineCollection.Id))
            {
                product.ProductCollections.Add(new ProductCollection
                {
                    ProductId = product.Id,
                    CollectionId = machineCollection.Id,
                    Product = product,
                    Collection = machineCollection,
                    SortOrder = product.Handle.EndsWith("30g", StringComparison.Ordinal) ? 10 : 20
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<Product> BuildStarterProducts()
    {
        var createdAtUtc = DateTimeOffset.UtcNow;
        var pimm30ProductId = Guid.NewGuid();
        var pimm50ProductId = Guid.NewGuid();

        return
        [
            new Product
            {
                Id = pimm30ProductId,
                Handle = "pneumatic-injection-molding-machine-30g",
                Title = "Pneumatic Injection Molding Machine 30g",
                Brand = "MALIEV",
                Summary = "Compact pneumatic injection molding machine for prototypes, classroom projects, recycled-plastic experiments, and small-batch plastic parts.",
                Description = """
                    Draft starter listing generated from MALIEV PIMM-30-125-200 flyer/manual material and storefront copy. Verify final price, images, accessories, lead time, and availability before publishing.

                    Key starter specs:
                    - Model: PIMM-30-125-200
                    - Shot size: 30g
                    - Injection force: 8,590 N, about 875 kgf at 0.7 MPa
                    - Cylinder stroke: 200 mm
                    - Max input air pressure: 0.7 MPa / 7 bar
                    - Temperature control: two independent PID-controlled heating zones
                    - Max temperature: 300 C
                    - Heater output: 2 x 300 W
                    - Power: 220 V, 1000 W
                    - Supported materials: HDPE, PET, PP, ABS, PLA
                    - Maximum mold size: 240 x 240 x 150 mm
                    - Machine dimensions: 425 x 420 x 885 mm
                    - Machine weight: about 60 kg
                    - Lead time starter value: 30 days

                    Customer positioning:
                    Built for small businesses, DIY makers, educators, students, recycling projects, prototyping, and low-volume plastic production.
                    """,
                ProductType = "Injection molding machine",
                Status = ProductStatus.Draft,
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = createdAtUtc,
                Variants =
                {
                    new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = pimm30ProductId,
                        Sku = "PIMM-30-125-200",
                        Title = "30g starter package",
                        PriceAmount = 99000,
                        Currency = "THB",
                        InventoryQuantity = 0,
                        IsActive = true,
                        OptionValuesJson = SerializeOptions(new Dictionary<string, string>
                        {
                            ["Shot size"] = "30g",
                            ["Max temperature"] = "300 C",
                            ["Heaters"] = "2 x 300 W",
                            ["Melt zone"] = "Aluminum",
                            ["Height"] = "885 mm",
                            ["Lead time"] = "30 days"
                        })
                    }
                }
            },
            new Product
            {
                Id = pimm50ProductId,
                Handle = "pneumatic-injection-molding-machine-50g",
                Title = "Pneumatic Injection Molding Machine 50g",
                Brand = "MALIEV",
                Summary = "New larger-capacity pneumatic injection molding machine with upgraded steel melt zone, higher-temperature operation, and 50g shot capacity.",
                Description = """
                    Draft starter listing based on the 30g PIMM-30-125-200 machine information plus MALIEV-provided 50g upgrade notes. Verify final model code, price, images, accessories, exact footprint, lead time, and availability before publishing.

                    Expected 50g improvements over the 30g machine:
                    - Shot size: 50g
                    - Max temperature: 350 C
                    - Heater output: 2 x 350 W hot runner heater bands
                    - Melt zone: upgraded from aluminum to steel for improved durability
                    - Machine height: 1015 mm
                    - Same compact pneumatic machine family for workshop and small-batch production

                    Keep as draft until final engineering specifications, pricing, product photography, and accessory bundle are confirmed.
                    """,
                ProductType = "Injection molding machine",
                Status = ProductStatus.Draft,
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = createdAtUtc,
                Variants =
                {
                    new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = pimm50ProductId,
                        Sku = "PIMM-50-DRAFT",
                        Title = "50g starter package",
                        PriceAmount = 0,
                        Currency = "THB",
                        InventoryQuantity = 0,
                        IsActive = false,
                        OptionValuesJson = SerializeOptions(new Dictionary<string, string>
                        {
                            ["Shot size"] = "50g",
                            ["Max temperature"] = "350 C",
                            ["Heaters"] = "2 x 350 W hot runner heater bands",
                            ["Melt zone"] = "Steel",
                            ["Height"] = "1015 mm",
                            ["Price"] = "TBD"
                        })
                    }
                }
            }
        ];
    }

    private static string SerializeOptions(IReadOnlyDictionary<string, string> options)
    {
        return JsonSerializer.Serialize(options);
    }
}
