using Maliev.CommerceService.Domain.Entities;

namespace Maliev.CommerceService.Application.Interfaces;

/// <summary>
/// Persistence abstraction for commerce aggregates.
/// </summary>
public interface ICommerceRepository
{
    /// <summary>Lists products.</summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> ListProductsAsync(string? query, string? collectionHandle, int page, int pageSize, bool includeDrafts, CancellationToken cancellationToken);

    /// <summary>Gets a product by handle.</summary>
    Task<Product?> GetProductByHandleAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Gets a product by id.</summary>
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a product.</summary>
    Task AddProductAsync(Product product, CancellationToken cancellationToken);

    /// <summary>Removes product variants.</summary>
    void RemoveProductVariants(IEnumerable<ProductVariant> variants);

    /// <summary>Removes product media.</summary>
    void RemoveProductMedia(IEnumerable<ProductMedia> media);

    /// <summary>Removes product collection links.</summary>
    void RemoveProductCollections(IEnumerable<ProductCollection> links);

    /// <summary>Lists collections.</summary>
    Task<IReadOnlyList<Collection>> ListCollectionsAsync(bool includeUnpublished, CancellationToken cancellationToken);

    /// <summary>Gets a collection by handle.</summary>
    Task<Collection?> GetCollectionByHandleAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Gets a collection by id.</summary>
    Task<Collection?> GetCollectionByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a collection.</summary>
    Task AddCollectionAsync(Collection collection, CancellationToken cancellationToken);

    /// <summary>Gets a variant by id.</summary>
    Task<ProductVariant?> GetVariantAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a cart.</summary>
    Task AddCartAsync(Cart cart, CancellationToken cancellationToken);

    /// <summary>Gets a cart by id.</summary>
    Task<Cart?> GetCartAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a cart line.</summary>
    void AddCartLine(CartLine line);

    /// <summary>Adds a checkout session.</summary>
    Task AddCheckoutSessionAsync(CheckoutSession checkoutSession, CancellationToken cancellationToken);

    /// <summary>Gets a checkout session by id.</summary>
    Task<CheckoutSession?> GetCheckoutSessionAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a store order.</summary>
    Task AddStoreOrderAsync(StoreOrder order, CancellationToken cancellationToken);

    /// <summary>Lists store orders.</summary>
    Task<IReadOnlyList<StoreOrder>> ListStoreOrdersAsync(Guid? customerId, CancellationToken cancellationToken);

    /// <summary>Gets a store order by id.</summary>
    Task<StoreOrder?> GetStoreOrderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Saves pending changes.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
