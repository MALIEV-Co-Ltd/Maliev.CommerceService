using Maliev.CommerceService.Application.Dtos;

namespace Maliev.CommerceService.Application.Services;

/// <summary>
/// Application service for storefront commerce.
/// </summary>
public interface ICommerceService
{
    /// <summary>Lists products.</summary>
    Task<PagedResponse<ProductSummaryResponse>> ListProductsAsync(string? query, string? collection, int page, int pageSize, bool includeDrafts, CancellationToken cancellationToken);

    /// <summary>Gets product by handle.</summary>
    Task<ProductResponse?> GetProductAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Creates product.</summary>
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);

    /// <summary>Updates product.</summary>
    Task<ProductResponse?> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);

    /// <summary>Updates product publishing status.</summary>
    Task<ProductResponse?> UpdateProductStatusAsync(Guid id, UpdateProductStatusRequest request, CancellationToken cancellationToken);

    /// <summary>Archives product.</summary>
    Task<bool> ArchiveProductAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Lists collections.</summary>
    Task<IReadOnlyList<CollectionResponse>> ListCollectionsAsync(bool includeUnpublished, CancellationToken cancellationToken);

    /// <summary>Gets a collection by handle.</summary>
    Task<CollectionResponse?> GetCollectionAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Creates collection.</summary>
    Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request, CancellationToken cancellationToken);

    /// <summary>Updates collection.</summary>
    Task<CollectionResponse?> UpdateCollectionAsync(Guid id, UpdateCollectionRequest request, CancellationToken cancellationToken);

    /// <summary>Unpublishes collection.</summary>
    Task<bool> UnpublishCollectionAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates cart.</summary>
    Task<CartResponse> CreateCartAsync(CreateCartRequest request, CancellationToken cancellationToken);

    /// <summary>Gets cart.</summary>
    Task<CartResponse?> GetCartAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Upserts cart line.</summary>
    Task<CartResponse?> UpsertCartLineAsync(Guid cartId, UpsertCartLineRequest request, CancellationToken cancellationToken);

    /// <summary>Removes cart line.</summary>
    Task<CartResponse?> RemoveCartLineAsync(Guid cartId, Guid lineId, CancellationToken cancellationToken);

    /// <summary>Creates checkout session.</summary>
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken);

    /// <summary>Gets checkout session.</summary>
    Task<CheckoutSessionResponse?> GetCheckoutSessionAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Completes checkout.</summary>
    Task<StoreOrderResponse?> CompleteCheckoutAsync(Guid checkoutSessionId, CancellationToken cancellationToken);

    /// <summary>Lists store orders.</summary>
    Task<IReadOnlyList<StoreOrderResponse>> ListOrdersAsync(Guid? customerId, CancellationToken cancellationToken);

    /// <summary>Gets store order.</summary>
    Task<StoreOrderResponse?> GetOrderAsync(Guid id, CancellationToken cancellationToken);
}
