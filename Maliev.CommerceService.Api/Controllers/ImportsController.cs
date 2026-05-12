using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CommerceService.Api.Authorization;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CommerceService.Api.Controllers;

/// <summary>
/// External catalog import endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("commerce/v{version:apiVersion}/imports")]
public sealed class ImportsController(IShopifyCatalogImportService shopifyCatalogImportService) : ControllerBase
{
    private readonly IShopifyCatalogImportService _shopifyCatalogImportService = shopifyCatalogImportService;

    /// <summary>
    /// Imports catalog data from the configured Shopify store.
    /// </summary>
    [HttpPost("shopify")]
    [RequirePermission(CommercePermissions.ImportsCreate)]
    public async Task<ActionResult<ShopifyCatalogImportResponse>> ImportShopifyCatalog(
        [FromBody] ShopifyCatalogImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _shopifyCatalogImportService.ImportAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Imports the configured Shopify pneumatic injection molding machine listing.
    /// </summary>
    [HttpPost("shopify/injection-molding-machine")]
    [RequirePermission(CommercePermissions.ImportsCreate)]
    public async Task<ActionResult<ShopifyCatalogImportResponse>> ImportInjectionMoldingMachine(
        [FromBody] ShopifyCatalogImportRequest? request,
        CancellationToken cancellationToken)
    {
        var importRequest = request ?? new ShopifyCatalogImportRequest();
        importRequest.SearchQuery = string.IsNullOrWhiteSpace(importRequest.SearchQuery)
            ? "injection molding machine"
            : importRequest.SearchQuery;
        AddIfMissing(importRequest.Keywords, "injection molding");
        AddIfMissing(importRequest.Keywords, "pneumatic injection");
        AddIfMissing(importRequest.Keywords, "injection-molding");
        importRequest.EnsureCollectionHandle = string.IsNullOrWhiteSpace(importRequest.EnsureCollectionHandle)
            ? "injection-molding-machines"
            : importRequest.EnsureCollectionHandle;
        importRequest.EnsureCollectionTitle = string.IsNullOrWhiteSpace(importRequest.EnsureCollectionTitle)
            ? "Injection Molding Machines"
            : importRequest.EnsureCollectionTitle;

        var result = await _shopifyCatalogImportService.ImportAsync(importRequest, cancellationToken);
        return Ok(result);
    }

    private static void AddIfMissing(List<string> values, string value)
    {
        if (!values.Any(existing => string.Equals(existing, value, StringComparison.OrdinalIgnoreCase)))
        {
            values.Add(value);
        }
    }
}
