namespace Maliev.CommerceService.Api.Authorization;

/// <summary>
/// Commerce service permissions.
/// </summary>
public static class CommercePermissions
{
    /// <summary>Read products.</summary>
    public const string ProductsRead = "commerce.products.read";

    /// <summary>Create products.</summary>
    public const string ProductsCreate = "commerce.products.create";

    /// <summary>Update products.</summary>
    public const string ProductsUpdate = "commerce.products.update";

    /// <summary>Delete products.</summary>
    public const string ProductsDelete = "commerce.products.delete";

    /// <summary>Read collections.</summary>
    public const string CollectionsRead = "commerce.collections.read";

    /// <summary>Create collections.</summary>
    public const string CollectionsCreate = "commerce.collections.create";

    /// <summary>Update collections.</summary>
    public const string CollectionsUpdate = "commerce.collections.update";

    /// <summary>Delete collections.</summary>
    public const string CollectionsDelete = "commerce.collections.delete";

    /// <summary>Read carts.</summary>
    public const string CartsRead = "commerce.carts.read";

    /// <summary>Create carts.</summary>
    public const string CartsCreate = "commerce.carts.create";

    /// <summary>Update carts.</summary>
    public const string CartsUpdate = "commerce.carts.update";

    /// <summary>Create checkout sessions.</summary>
    public const string CheckoutsCreate = "commerce.checkouts.create";

    /// <summary>Read orders.</summary>
    public const string OrdersRead = "commerce.orders.read";

    /// <summary>Create orders.</summary>
    public const string OrdersCreate = "commerce.orders.create";
}
