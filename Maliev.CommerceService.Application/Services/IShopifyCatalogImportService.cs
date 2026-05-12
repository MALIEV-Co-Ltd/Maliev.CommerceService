using Maliev.CommerceService.Application.Dtos;

namespace Maliev.CommerceService.Application.Services;

/// <summary>
/// Imports storefront catalog data from Shopify into the Commerce catalog.
/// </summary>
public interface IShopifyCatalogImportService
{
    /// <summary>
    /// Imports products, variants, images, and collections from Shopify.
    /// </summary>
    Task<ShopifyCatalogImportResponse> ImportAsync(ShopifyCatalogImportRequest request, CancellationToken cancellationToken);
}
