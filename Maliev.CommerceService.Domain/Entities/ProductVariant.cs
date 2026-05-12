using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Sellable product variant.
/// </summary>
public class ProductVariant
{
    /// <summary>Gets or sets the variant identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the SKU.</summary>
    [Required]
    [MaxLength(120)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant title.</summary>
    [Required]
    [MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant price amount.</summary>
    public decimal PriceAmount { get; set; }

    /// <summary>Gets or sets the ISO currency code.</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the available inventory quantity.</summary>
    public int InventoryQuantity { get; set; }

    /// <summary>Gets or sets a value indicating whether the variant can be purchased.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets a JSON object with option values such as color or size.</summary>
    public string? OptionValuesJson { get; set; }

    /// <summary>Gets or sets the owning product.</summary>
    public Product? Product { get; set; }
}
