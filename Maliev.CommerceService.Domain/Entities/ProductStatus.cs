namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Product publishing status.
/// </summary>
public static class ProductStatus
{
    /// <summary>Draft product hidden from the storefront.</summary>
    public const string Draft = "Draft";

    /// <summary>Published product visible on the storefront.</summary>
    public const string Published = "Published";

    /// <summary>Archived product retained for historical orders.</summary>
    public const string Archived = "Archived";
}
