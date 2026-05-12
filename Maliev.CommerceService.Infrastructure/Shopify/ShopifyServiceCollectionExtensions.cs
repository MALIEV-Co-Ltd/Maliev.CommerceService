using Maliev.CommerceService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.CommerceService.Infrastructure.Shopify;

/// <summary>
/// Dependency injection registration for Shopify catalog imports.
/// </summary>
public static class ShopifyServiceCollectionExtensions
{
    /// <summary>
    /// Adds Shopify Admin API catalog import services.
    /// </summary>
    public static IServiceCollection AddShopifyCatalogImport(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Shopify");
        services.Configure<ShopifyOptions>(options =>
        {
            options.StoreDomain = section["StoreDomain"] ?? string.Empty;
            options.AdminAccessToken = section["AdminAccessToken"] ?? string.Empty;
            options.AdminApiVersion = section["AdminApiVersion"] ?? options.AdminApiVersion;
        });
        services.AddSingleton<HttpClient>();
        services.AddScoped<IShopifyAdminClient, ShopifyAdminClient>();
        services.AddScoped<IShopifyCatalogImportService, ShopifyCatalogImportService>();
        return services;
    }
}
