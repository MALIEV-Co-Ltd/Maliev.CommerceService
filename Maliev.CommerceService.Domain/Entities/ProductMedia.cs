using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Product image or media asset.
/// </summary>
public class ProductMedia
{
    /// <summary>Gets or sets the media identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the media URL.</summary>
    [Required]
    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the accessible alt text.</summary>
    [MaxLength(240)]
    public string? AltText { get; set; }

    /// <summary>Gets or sets the sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the owning product.</summary>
    public Product? Product { get; set; }
}
