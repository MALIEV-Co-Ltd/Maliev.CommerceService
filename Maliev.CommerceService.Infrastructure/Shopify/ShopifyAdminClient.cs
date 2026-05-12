using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Maliev.CommerceService.Infrastructure.Shopify;

internal sealed class ShopifyAdminClient(HttpClient httpClient, IOptions<ShopifyOptions> options) : IShopifyAdminClient
{
    private const string CatalogQuery = """
        query CommerceCatalogImport($first: Int!, $after: String, $query: String) {
          shop {
            currencyCode
          }
          products(first: $first, after: $after, query: $query) {
            pageInfo {
              hasNextPage
              endCursor
            }
            nodes {
              id
              handle
              title
              vendor
              productType
              descriptionHtml
              status
              tags
              variants(first: 100) {
                nodes {
                  id
                  title
                  sku
                  price
                  inventoryQuantity
                  selectedOptions {
                    name
                    value
                  }
                }
              }
              media(first: 20) {
                nodes {
                  alt
                  mediaContentType
                  preview {
                    image {
                      url
                      altText
                    }
                  }
                }
              }
              collections(first: 20) {
                nodes {
                  id
                  handle
                  title
                  description
                }
              }
            }
          }
        }
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient = httpClient;
    private readonly ShopifyOptions _options = options.Value;

    public async Task<ShopifyCatalogPage> GetCatalogPageAsync(int first, string? after, string? query, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildGraphQlUri());
        request.Headers.Add("X-Shopify-Access-Token", _options.AdminAccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(
            new ShopifyGraphQlRequest
            {
                Query = CatalogQuery,
                Variables = new Dictionary<string, object?>
                {
                    ["first"] = first,
                    ["after"] = after,
                    ["query"] = string.IsNullOrWhiteSpace(query) ? null : query.Trim()
                }
            },
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ShopifyGraphQlResponse<ShopifyCatalogPage>>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Shopify returned an empty response.");

        if (payload.Errors is { Count: > 0 })
        {
            var messages = string.Join("; ", payload.Errors.Select(error => error.Message));
            throw new InvalidOperationException($"Shopify catalog query failed: {messages}");
        }

        return payload.Data ?? throw new InvalidOperationException("Shopify catalog response did not include data.");
    }

    private Uri BuildGraphQlUri()
    {
        var domain = _options.StoreDomain.Trim();
        if (!domain.EndsWith(".myshopify.com", StringComparison.OrdinalIgnoreCase)
            && !domain.Contains('.', StringComparison.Ordinal))
        {
            domain = $"{domain}.myshopify.com";
        }

        return new Uri($"https://{domain}/admin/api/{_options.AdminApiVersion.Trim()}/graphql.json");
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.StoreDomain))
        {
            throw new InvalidOperationException("Shopify store domain is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.AdminAccessToken))
        {
            throw new InvalidOperationException("Shopify Admin access token is not configured.");
        }
    }
}
