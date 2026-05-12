using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Storefront product collection.
/// </summary>
public class Collection
{
    /// <summary>Gets or sets the collection identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the collection handle.</summary>
    [Required]
    [MaxLength(160)]
    public string Handle { get; set; } = string.Empty;

    /// <summary>Gets or sets the collection title.</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the collection description.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the collection is visible.</summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>Gets the product collection links.</summary>
    public List<ProductCollection> ProductCollections { get; } = [];
}
