using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Interfaces;
using Maliev.CommerceService.Application.Services;
using Maliev.CommerceService.Domain.Entities;

namespace Maliev.CommerceService.Infrastructure.Shopify;

internal sealed partial class ShopifyCatalogImportService(IShopifyAdminClient shopifyClient, ICommerceRepository repository) : IShopifyCatalogImportService
{
    private readonly IShopifyAdminClient _shopifyClient = shopifyClient;
    private readonly ICommerceRepository _repository = repository;

    /// <inheritdoc />
    public async Task<ShopifyCatalogImportResponse> ImportAsync(ShopifyCatalogImportRequest request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var ensuredCollectionHandle = string.IsNullOrWhiteSpace(request.EnsureCollectionHandle)
            ? null
            : NormalizeHandle(request.EnsureCollectionHandle);
        string? cursor = null;
        var productsRead = 0;
        var productsCreated = 0;
        var productsUpdated = 0;
        var productsSkipped = 0;
        var collectionsCreated = 0;
        var warnings = new List<string>();
        var createdCollectionHandles = new HashSet<string>(StringComparer.Ordinal);

        do
        {
            var page = await _shopifyClient.GetCatalogPageAsync(pageSize, cursor, request.SearchQuery, cancellationToken);
            var currency = NormalizeCurrency(page.Shop.CurrencyCode);

            foreach (var shopifyProduct in page.Products.Nodes)
            {
                productsRead++;
                if (string.IsNullOrWhiteSpace(shopifyProduct.Handle) || string.IsNullOrWhiteSpace(shopifyProduct.Title))
                {
                    productsSkipped++;
                    warnings.Add($"Skipped Shopify product {shopifyProduct.Id} because it has no handle or title.");
                    continue;
                }

                if (!MatchesFilters(shopifyProduct, request))
                {
                    productsSkipped++;
                    continue;
                }

                var collectionHandles = new List<string>();
                var addedCollectionForProduct = false;
                foreach (var shopifyCollection in shopifyProduct.Collections.Nodes)
                {
                    if (string.IsNullOrWhiteSpace(shopifyCollection.Handle) || string.IsNullOrWhiteSpace(shopifyCollection.Title))
                    {
                        continue;
                    }

                    var collectionHandle = NormalizeHandle(shopifyCollection.Handle);
                    collectionHandles.Add(collectionHandle);
                    var existingCollection = await _repository.GetCollectionByHandleAsync(collectionHandle, cancellationToken);
                    if (existingCollection is null && createdCollectionHandles.Add(collectionHandle))
                    {
                        collectionsCreated++;
                        if (!request.DryRun)
                        {
                            await _repository.AddCollectionAsync(new Collection
                            {
                                Id = Guid.NewGuid(),
                                Handle = collectionHandle,
                                Title = shopifyCollection.Title.Trim(),
                                Description = Truncate(shopifyCollection.Description, 1000),
                                IsPublished = true
                            }, cancellationToken);
                            addedCollectionForProduct = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ensuredCollectionHandle))
                {
                    collectionHandles.Add(ensuredCollectionHandle);
                    var ensuredCollection = await _repository.GetCollectionByHandleAsync(ensuredCollectionHandle, cancellationToken);
                    if (ensuredCollection is null && createdCollectionHandles.Add(ensuredCollectionHandle))
                    {
                        collectionsCreated++;
                        if (!request.DryRun)
                        {
                            await _repository.AddCollectionAsync(new Collection
                            {
                                Id = Guid.NewGuid(),
                                Handle = ensuredCollectionHandle,
                                Title = string.IsNullOrWhiteSpace(request.EnsureCollectionTitle)
                                    ? shopifyProduct.ProductType ?? "Imported Shopify products"
                                    : request.EnsureCollectionTitle.Trim(),
                                Description = "Imported from Shopify.",
                                IsPublished = true
                            }, cancellationToken);
                            addedCollectionForProduct = true;
                        }
                    }
                }

                if (addedCollectionForProduct)
                {
                    await _repository.SaveChangesAsync(cancellationToken);
                }

                var existingProduct = await _repository.GetProductByHandleAsync(NormalizeHandle(shopifyProduct.Handle), cancellationToken);
                if (existingProduct is null)
                {
                    productsCreated++;
                    if (!request.DryRun)
                    {
                        var product = new Product { Id = Guid.NewGuid(), CreatedAtUtc = DateTimeOffset.UtcNow };
                        await ApplyProductAsync(product, shopifyProduct, currency, collectionHandles, cancellationToken);
                        await _repository.AddProductAsync(product, cancellationToken);
                    }
                }
                else
                {
                    productsUpdated++;
                    if (!request.DryRun)
                    {
                        await ApplyProductAsync(existingProduct, shopifyProduct, currency, collectionHandles, cancellationToken);
                    }
                }
            }

            if (!request.DryRun)
            {
                await _repository.SaveChangesAsync(cancellationToken);
            }

            cursor = page.Products.PageInfo.EndCursor;
            if (!page.Products.PageInfo.HasNextPage)
            {
                cursor = null;
            }
        }
        while (cursor is not null);

        return new ShopifyCatalogImportResponse(
            productsRead,
            productsCreated,
            productsUpdated,
            productsSkipped,
            collectionsCreated,
            request.DryRun,
            warnings);
    }

    private static bool MatchesFilters(ShopifyProductNode product, ShopifyCatalogImportRequest request)
    {
        if (request.ProductHandles.Count == 0
            && request.ProductTypes.Count == 0
            && request.Tags.Count == 0
            && request.CollectionHandles.Count == 0
            && request.Keywords.Count == 0)
        {
            return true;
        }

        if (request.ProductHandles.Count > 0
            && !request.ProductHandles.Any(handle => string.Equals(NormalizeHandle(handle), NormalizeHandle(product.Handle), StringComparison.Ordinal)))
        {
            return false;
        }

        if (request.ProductTypes.Count > 0
            && !request.ProductTypes.Any(type => ContainsNormalized(product.ProductType, type)))
        {
            return false;
        }

        if (request.Tags.Count > 0
            && !request.Tags.Any(required => product.Tags.Any(tag => ContainsNormalized(tag, required))))
        {
            return false;
        }

        if (request.CollectionHandles.Count > 0)
        {
            var productCollectionHandles = product.Collections.Nodes
                .Select(collection => NormalizeHandle(collection.Handle))
                .ToHashSet(StringComparer.Ordinal);

            if (!request.CollectionHandles.Any(handle => productCollectionHandles.Contains(NormalizeHandle(handle))))
            {
                return false;
            }
        }

        if (request.Keywords.Count > 0)
        {
            var searchable = string.Join(
                " ",
                product.Handle,
                product.Title,
                product.ProductType,
                string.Join(" ", product.Tags),
                string.Join(" ", product.Collections.Nodes.Select(collection => collection.Handle)),
                string.Join(" ", product.Collections.Nodes.Select(collection => collection.Title)));

            if (!request.Keywords.Any(keyword => ContainsNormalized(searchable, keyword)))
            {
                return false;
            }
        }

        return true;
    }

    private async Task ApplyProductAsync(
        Product product,
        ShopifyProductNode shopifyProduct,
        string currency,
        IEnumerable<string> collectionHandles,
        CancellationToken cancellationToken)
    {
        product.Handle = NormalizeHandle(shopifyProduct.Handle);
        product.Title = shopifyProduct.Title.Trim();
        product.Brand = string.IsNullOrWhiteSpace(shopifyProduct.Vendor) ? null : Truncate(shopifyProduct.Vendor, 120);
        product.Description = shopifyProduct.DescriptionHtml?.Trim() ?? string.Empty;
        product.Summary = BuildSummary(shopifyProduct.DescriptionHtml, shopifyProduct.Tags);
        product.ProductType = string.IsNullOrWhiteSpace(shopifyProduct.ProductType) ? "Product" : Truncate(shopifyProduct.ProductType, 120)!;
        product.Status = MapProductStatus(shopifyProduct.Status);
        product.UpdatedAtUtc = DateTimeOffset.UtcNow;

        SyncVariants(product, shopifyProduct, currency);
        SyncMedia(product, shopifyProduct);
        await SyncCollectionsAsync(product, collectionHandles, cancellationToken);
    }

    private static void SyncVariants(Product product, ShopifyProductNode shopifyProduct, string currency)
    {
        var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var shopifyVariant in shopifyProduct.Variants.Nodes)
        {
            var sku = BuildSku(shopifyProduct, shopifyVariant);
            seenSkus.Add(sku);
            var variant = product.Variants.FirstOrDefault(existing => string.Equals(existing.Sku, sku, StringComparison.OrdinalIgnoreCase));
            if (variant is null)
            {
                variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Sku = sku
                };
                product.Variants.Add(variant);
            }

            variant.Title = string.IsNullOrWhiteSpace(shopifyVariant.Title) ? "Default" : Truncate(shopifyVariant.Title, 240)!;
            variant.PriceAmount = ParsePrice(shopifyVariant.Price);
            variant.Currency = currency;
            variant.InventoryQuantity = Math.Max(0, shopifyVariant.InventoryQuantity);
            variant.IsActive = true;
            variant.OptionValuesJson = shopifyVariant.SelectedOptions.Count == 0
                ? null
                : JsonSerializer.Serialize(shopifyVariant.SelectedOptions.ToDictionary(option => option.Name, option => option.Value));
        }

        foreach (var variant in product.Variants.Where(existing => !seenSkus.Contains(existing.Sku)).ToList())
        {
            variant.IsActive = false;
        }
    }

    private static void SyncMedia(Product product, ShopifyProductNode shopifyProduct)
    {
        product.Media.Clear();
        var sortOrder = 0;
        foreach (var media in shopifyProduct.Media.Nodes)
        {
            var image = media.Preview?.Image;
            if (!string.Equals(media.MediaContentType, "IMAGE", StringComparison.OrdinalIgnoreCase)
                || image is null
                || string.IsNullOrWhiteSpace(image.Url))
            {
                continue;
            }

            product.Media.Add(new ProductMedia
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = image.Url.Trim(),
                AltText = Truncate(media.Alt ?? image.AltText, 240),
                SortOrder = sortOrder++
            });
        }
    }

    private async Task SyncCollectionsAsync(Product product, IEnumerable<string> collectionHandles, CancellationToken cancellationToken)
    {
        product.ProductCollections.Clear();
        var sortOrder = 0;
        foreach (var rawHandle in collectionHandles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var collection = await _repository.GetCollectionByHandleAsync(rawHandle, cancellationToken);
            if (collection is null)
            {
                continue;
            }

            product.ProductCollections.Add(new ProductCollection
            {
                ProductId = product.Id,
                CollectionId = collection.Id,
                Collection = collection,
                SortOrder = sortOrder++
            });
        }
    }

    private static string BuildSummary(string? html, IReadOnlyCollection<string> tags)
    {
        var text = StripHtml(html);
        if (string.IsNullOrWhiteSpace(text) && tags.Count > 0)
        {
            text = string.Join(", ", tags.Take(8));
        }

        return Truncate(text, 500) ?? string.Empty;
    }

    private static string BuildSku(ShopifyProductNode product, ShopifyVariantNode variant)
    {
        var sku = string.IsNullOrWhiteSpace(variant.Sku)
            ? $"{product.Handle}-{ExtractShopifyId(variant.Id)}"
            : variant.Sku;

        return Truncate(sku.Trim(), 120) ?? ExtractShopifyId(variant.Id);
    }

    private static string ExtractShopifyId(string gid)
    {
        var index = gid.LastIndexOf("/", StringComparison.Ordinal);
        return index >= 0 && index < gid.Length - 1 ? gid[(index + 1)..] : gid;
    }

    private static decimal ParsePrice(string price)
    {
        return decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? decimal.Round(parsed, 2, MidpointRounding.AwayFromZero)
            : 0m;
    }

    private static string MapProductStatus(string status)
    {
        return string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase)
            ? ProductStatus.Published
            : ProductStatus.Draft;
    }

    private static string NormalizeHandle(string handle)
    {
        return handle.Trim().ToLowerInvariant().Replace(' ', '-');
    }

    private static string NormalizeCurrency(string currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "THB" : currency.Trim().ToUpperInvariant();
    }

    private static bool ContainsNormalized(string? value, string expected)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        return value.NormalizeForSearch().Contains(expected.NormalizeForSearch(), StringComparison.Ordinal);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return WhitespaceRegex().Replace(HtmlTagRegex().Replace(html, " "), " ").Trim();
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}

internal static class ShopifyCatalogImportSearchExtensions
{
    public static string NormalizeForSearch(this string value)
    {
        return value.Trim().ToLowerInvariant().Replace('-', ' ');
    }
}
