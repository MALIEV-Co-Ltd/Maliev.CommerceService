using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Storefront product sold through MALIEV Web.
/// </summary>
public class Product
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the URL handle.</summary>
    [Required]
    [MaxLength(160)]
    public string Handle { get; set; } = string.Empty;

    /// <summary>Gets or sets the product title.</summary>
    [Required]
    [MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the product brand.</summary>
    [MaxLength(120)]
    public string? Brand { get; set; }

    /// <summary>Gets or sets the product summary.</summary>
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets the detailed product description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the product type.</summary>
    [Required]
    [MaxLength(120)]
    public string ProductType { get; set; } = "Product";

    /// <summary>Gets or sets the product status.</summary>
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = ProductStatus.Draft;

    /// <summary>Gets or sets the created timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the updated timestamp.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the product variants.</summary>
    public List<ProductVariant> Variants { get; } = [];

    /// <summary>Gets the product media.</summary>
    public List<ProductMedia> Media { get; } = [];

    /// <summary>Gets the product collection links.</summary>
    public List<ProductCollection> ProductCollections { get; } = [];
}
