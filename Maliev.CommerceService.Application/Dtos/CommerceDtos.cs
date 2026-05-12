using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Application.Dtos;

/// <summary>
/// Generic paginated response.
/// </summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

/// <summary>
/// Product response.
/// </summary>
public sealed record ProductResponse(
    Guid Id,
    string Handle,
    string Title,
    string? Brand,
    string Summary,
    string Description,
    string ProductType,
    string Status,
    IReadOnlyList<ProductVariantResponse> Variants,
    IReadOnlyList<ProductMediaResponse> Media,
    IReadOnlyList<CollectionSummaryResponse> Collections);

/// <summary>
/// Product summary response.
/// </summary>
public sealed record ProductSummaryResponse(
    Guid Id,
    string Handle,
    string Title,
    string? Brand,
    string Summary,
    string ProductType,
    string Status,
    decimal StartingPrice,
    string Currency,
    string? ThumbnailUrl);

/// <summary>
/// Product variant response.
/// </summary>
public sealed record ProductVariantResponse(Guid Id, string Sku, string Title, decimal PriceAmount, string Currency, int InventoryQuantity, bool IsActive, string? OptionValuesJson);

/// <summary>
/// Product media response.
/// </summary>
public sealed record ProductMediaResponse(Guid Id, string Url, string? AltText, int SortOrder);

/// <summary>
/// Collection response.
/// </summary>
public sealed record CollectionResponse(Guid Id, string Handle, string Title, string? Description, bool IsPublished);

/// <summary>
/// Collection summary response.
/// </summary>
public sealed record CollectionSummaryResponse(Guid Id, string Handle, string Title);

/// <summary>
/// Cart response.
/// </summary>
public sealed record CartResponse(Guid Id, Guid? CustomerId, string? AnonymousKey, string Status, string Currency, IReadOnlyList<CartLineResponse> Lines, decimal TotalAmount);

/// <summary>
/// Cart line response.
/// </summary>
public sealed record CartLineResponse(Guid Id, Guid ProductVariantId, string Sku, string Title, int Quantity, decimal UnitPriceAmount, string Currency, decimal LineTotal);

/// <summary>
/// Checkout session response.
/// </summary>
public sealed record CheckoutSessionResponse(Guid Id, Guid CartId, Guid CustomerId, string Status, decimal TotalAmount, string Currency, DateTimeOffset ExpiresAtUtc);

/// <summary>
/// Store order response.
/// </summary>
public sealed record StoreOrderResponse(Guid Id, string OrderNumber, Guid CustomerId, string Status, decimal TotalAmount, string Currency, IReadOnlyList<StoreOrderLineResponse> Lines, DateTimeOffset CreatedAtUtc);

/// <summary>
/// Store order line response.
/// </summary>
public sealed record StoreOrderLineResponse(Guid Id, Guid ProductVariantId, string Sku, string Title, int Quantity, decimal UnitPriceAmount, string Currency, decimal LineTotal);

/// <summary>
/// Product creation request.
/// </summary>
public class CreateProductRequest
{
    /// <summary>Gets or sets the product handle.</summary>
    [Required]
    [MaxLength(160)]
    public string Handle { get; set; } = string.Empty;

    /// <summary>Gets or sets the product title.</summary>
    [Required]
    [MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the brand.</summary>
    [MaxLength(120)]
    public string? Brand { get; set; }

    /// <summary>Gets or sets the summary.</summary>
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the product type.</summary>
    [Required]
    [MaxLength(120)]
    public string ProductType { get; set; } = "Product";

    /// <summary>Gets or sets the status.</summary>
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = "Draft";

    /// <summary>Gets or sets variants.</summary>
    [MinLength(1)]
    public List<CreateProductVariantRequest> Variants { get; set; } = [];

    /// <summary>Gets or sets media.</summary>
    public List<CreateProductMediaRequest> Media { get; set; } = [];

    /// <summary>Gets or sets collection handles.</summary>
    public List<string> CollectionHandles { get; set; } = [];
}

/// <summary>
/// Product update request.
/// </summary>
public sealed class UpdateProductRequest : CreateProductRequest
{
}

/// <summary>
/// Product status update request.
/// </summary>
public sealed class UpdateProductStatusRequest
{
    /// <summary>Gets or sets the product status.</summary>
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = "Draft";
}

/// <summary>
/// Product variant creation request.
/// </summary>
public sealed class CreateProductVariantRequest
{
    /// <summary>Gets or sets the SKU.</summary>
    [Required]
    [MaxLength(120)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant title.</summary>
    [Required]
    [MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the price amount.</summary>
    [Range(0.01, 999999999)]
    public decimal PriceAmount { get; set; }

    /// <summary>Gets or sets the currency.</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets inventory quantity.</summary>
    [Range(0, int.MaxValue)]
    public int InventoryQuantity { get; set; }

    /// <summary>Gets or sets option values as JSON.</summary>
    public string? OptionValuesJson { get; set; }
}

/// <summary>
/// Product media creation request.
/// </summary>
public sealed class CreateProductMediaRequest
{
    /// <summary>Gets or sets the URL.</summary>
    [Required]
    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets alt text.</summary>
    [MaxLength(240)]
    public string? AltText { get; set; }

    /// <summary>Gets or sets sort order.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Collection creation request.
/// </summary>
public class CreateCollectionRequest
{
    /// <summary>Gets or sets the handle.</summary>
    [Required]
    [MaxLength(160)]
    public string Handle { get; set; } = string.Empty;

    /// <summary>Gets or sets the title.</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Gets or sets whether the collection is published.</summary>
    public bool IsPublished { get; set; } = true;
}

/// <summary>
/// Collection update request.
/// </summary>
public sealed class UpdateCollectionRequest : CreateCollectionRequest
{
}

/// <summary>
/// Cart creation request.
/// </summary>
public sealed class CreateCartRequest
{
    /// <summary>Gets or sets customer id.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets anonymous key.</summary>
    [MaxLength(160)]
    public string? AnonymousKey { get; set; }

    /// <summary>Gets or sets currency.</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "THB";
}

/// <summary>
/// Cart line mutation request.
/// </summary>
public sealed class UpsertCartLineRequest
{
    /// <summary>Gets or sets product variant id.</summary>
    [Required]
    public Guid ProductVariantId { get; set; }

    /// <summary>Gets or sets quantity.</summary>
    [Range(1, 999)]
    public int Quantity { get; set; }
}

/// <summary>
/// Checkout session creation request.
/// </summary>
public sealed class CreateCheckoutSessionRequest
{
    /// <summary>Gets or sets cart id.</summary>
    [Required]
    public Guid CartId { get; set; }

    /// <summary>Gets or sets customer id.</summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets shipping address snapshot JSON.</summary>
    public string? ShippingAddressJson { get; set; }

    /// <summary>Gets or sets billing address snapshot JSON.</summary>
    public string? BillingAddressJson { get; set; }
}
