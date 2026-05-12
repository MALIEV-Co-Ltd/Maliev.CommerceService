namespace Maliev.CommerceService.Infrastructure.Shopify;

internal interface IShopifyAdminClient
{
    Task<ShopifyCatalogPage> GetCatalogPageAsync(int first, string? after, string? query, CancellationToken cancellationToken);
}
