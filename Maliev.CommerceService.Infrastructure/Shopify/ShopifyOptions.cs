namespace Maliev.CommerceService.Infrastructure.Shopify;

/// <summary>
/// Configuration for Shopify Admin API imports.
/// </summary>
public sealed class ShopifyOptions
{
    /// <summary>Gets or sets the myshopify domain.</summary>
    public string StoreDomain { get; set; } = string.Empty;

    /// <summary>Gets or sets the Shopify Admin API access token.</summary>
    public string AdminAccessToken { get; set; } = string.Empty;

    /// <summary>Gets or sets the Shopify Admin API version.</summary>
    public string AdminApiVersion { get; set; } = "2026-04";
}
