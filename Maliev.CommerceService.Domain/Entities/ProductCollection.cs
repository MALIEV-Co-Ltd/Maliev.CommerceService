namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Join entity between products and collections.
/// </summary>
public class ProductCollection
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the collection identifier.</summary>
    public Guid CollectionId { get; set; }

    /// <summary>Gets or sets the sort order inside the collection.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the product.</summary>
    public Product? Product { get; set; }

    /// <summary>Gets or sets the collection.</summary>
    public Collection? Collection { get; set; }
}
