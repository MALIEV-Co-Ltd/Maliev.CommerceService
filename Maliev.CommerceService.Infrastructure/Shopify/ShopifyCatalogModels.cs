using System.Text.Json.Serialization;

namespace Maliev.CommerceService.Infrastructure.Shopify;

internal sealed class ShopifyGraphQlRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    public Dictionary<string, object?> Variables { get; set; } = [];
}

internal sealed class ShopifyGraphQlResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<ShopifyGraphQlError>? Errors { get; set; }
}

internal sealed class ShopifyGraphQlError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

internal sealed class ShopifyCatalogPage
{
    [JsonPropertyName("shop")]
    public ShopifyShopNode Shop { get; set; } = new();

    [JsonPropertyName("products")]
    public ShopifyProductConnection Products { get; set; } = new();
}

internal sealed class ShopifyShopNode
{
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = "THB";
}

internal sealed class ShopifyProductConnection
{
    [JsonPropertyName("pageInfo")]
    public ShopifyPageInfo PageInfo { get; set; } = new();

    [JsonPropertyName("nodes")]
    public List<ShopifyProductNode> Nodes { get; set; } = [];
}

internal sealed class ShopifyPageInfo
{
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonPropertyName("endCursor")]
    public string? EndCursor { get; set; }
}

internal sealed class ShopifyProductNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }

    [JsonPropertyName("productType")]
    public string? ProductType { get; set; }

    [JsonPropertyName("descriptionHtml")]
    public string? DescriptionHtml { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("variants")]
    public ShopifyVariantConnection Variants { get; set; } = new();

    [JsonPropertyName("media")]
    public ShopifyMediaConnection Media { get; set; } = new();

    [JsonPropertyName("collections")]
    public ShopifyCollectionConnection Collections { get; set; } = new();
}

internal sealed class ShopifyVariantConnection
{
    [JsonPropertyName("nodes")]
    public List<ShopifyVariantNode> Nodes { get; set; } = [];
}

internal sealed class ShopifyVariantNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; } = "0";

    [JsonPropertyName("inventoryQuantity")]
    public int InventoryQuantity { get; set; }

    [JsonPropertyName("selectedOptions")]
    public List<ShopifySelectedOptionNode> SelectedOptions { get; set; } = [];
}

internal sealed class ShopifySelectedOptionNode
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

internal sealed class ShopifyMediaConnection
{
    [JsonPropertyName("nodes")]
    public List<ShopifyMediaNode> Nodes { get; set; } = [];
}

internal sealed class ShopifyMediaNode
{
    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    [JsonPropertyName("mediaContentType")]
    public string MediaContentType { get; set; } = string.Empty;

    [JsonPropertyName("preview")]
    public ShopifyMediaPreview? Preview { get; set; }
}

internal sealed class ShopifyMediaPreview
{
    [JsonPropertyName("image")]
    public ShopifyImageNode? Image { get; set; }
}

internal sealed class ShopifyImageNode
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("altText")]
    public string? AltText { get; set; }
}

internal sealed class ShopifyCollectionConnection
{
    [JsonPropertyName("nodes")]
    public List<ShopifyCollectionNode> Nodes { get; set; } = [];
}

internal sealed class ShopifyCollectionNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
