using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Bill of materials item used to build or package a storefront product.
/// </summary>
public class ProductBomItem
{
    /// <summary>Gets or sets the BOM item identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the material, component, or consumable name.</summary>
    [Required]
    [MaxLength(240)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>Gets or sets the internal or supplier part number.</summary>
    [MaxLength(120)]
    public string? PartNumber { get; set; }

    /// <summary>Gets or sets the top-level assembly where this item is used.</summary>
    [MaxLength(160)]
    public string? AssemblyName { get; set; }

    /// <summary>Gets or sets the subassembly where this item is used.</summary>
    [MaxLength(160)]
    public string? SubassemblyName { get; set; }

    /// <summary>Gets or sets the item specification, grade, color, size, or supplier reference.</summary>
    [MaxLength(500)]
    public string? Specification { get; set; }

    /// <summary>Gets or sets an image URL for visual identification.</summary>
    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets a drawing or datasheet URL.</summary>
    [MaxLength(1000)]
    public string? DrawingUrl { get; set; }

    /// <summary>Gets or sets the preferred supplier name.</summary>
    [MaxLength(240)]
    public string? SupplierName { get; set; }

    /// <summary>Gets or sets the supplier product or sourcing URL.</summary>
    [MaxLength(1000)]
    public string? SupplierUrl { get; set; }

    /// <summary>Gets or sets the supplier lead time in days.</summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>Gets or sets the internal sourcing buffer in days.</summary>
    public int? SourcingTimeDays { get; set; }

    /// <summary>Gets or sets the quantity used by one sellable product unit.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Gets or sets the unit of measure.</summary>
    [Required]
    [MaxLength(40)]
    public string Unit { get; set; } = "pcs";

    /// <summary>Gets or sets the expected unit cost.</summary>
    public decimal UnitCost { get; set; }

    /// <summary>Gets or sets the ISO currency code.</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets internal notes about the item.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Gets or sets the owning product.</summary>
    public Product? Product { get; set; }
}
