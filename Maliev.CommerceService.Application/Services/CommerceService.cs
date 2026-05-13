using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Interfaces;
using Maliev.CommerceService.Domain.Entities;

namespace Maliev.CommerceService.Application.Services;

/// <inheritdoc />
public sealed class CommerceService(ICommerceRepository repository) : ICommerceService
{
    private readonly ICommerceRepository _repository = repository;

    /// <inheritdoc />
    public async Task<PagedResponse<ProductSummaryResponse>> ListProductsAsync(string? query, string? collection, int page, int pageSize, bool includeDrafts, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalCount) = await _repository.ListProductsAsync(query, collection, page, pageSize, includeDrafts, cancellationToken);
        return new PagedResponse<ProductSummaryResponse>(items.Select(item => item.ToSummaryResponse()).ToList(), page, pageSize, totalCount);
    }

    /// <inheritdoc />
    public async Task<ProductResponse?> GetProductAsync(string handle, CancellationToken cancellationToken)
    {
        var product = await _repository.GetProductByHandleAsync(NormalizeHandle(handle), cancellationToken);
        return product?.ToResponse();
    }

    /// <inheritdoc />
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = BuildProduct(request, new Product { Id = Guid.NewGuid(), CreatedAtUtc = DateTimeOffset.UtcNow });
        await LinkCollectionsAsync(product, request.CollectionHandles, cancellationToken);
        await _repository.AddProductAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    /// <inheritdoc />
    public async Task<ProductResponse?> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        ApplyProductBasics(request, product);
        SyncVariants(product, request.Variants);
        SyncMedia(product, request.Media);
        await SyncCollectionsAsync(product, request.CollectionHandles, cancellationToken);
        product.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    /// <inheritdoc />
    public async Task<ProductResponse?> UpdateProductStatusAsync(Guid id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Status = NormalizeProductStatus(request.Status);
        product.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    /// <inheritdoc />
    public async Task<bool> ArchiveProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Status = ProductStatus.Archived;
        foreach (var variant in product.Variants)
        {
            variant.IsActive = false;
        }

        product.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CollectionResponse>> ListCollectionsAsync(bool includeUnpublished, CancellationToken cancellationToken)
    {
        var collections = await _repository.ListCollectionsAsync(includeUnpublished, cancellationToken);
        return collections.Select(collection => collection.ToResponse()).ToList();
    }

    /// <inheritdoc />
    public async Task<CollectionResponse?> GetCollectionAsync(string handle, CancellationToken cancellationToken)
    {
        var collection = await _repository.GetCollectionByHandleAsync(NormalizeHandle(handle), cancellationToken);
        return collection?.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request, CancellationToken cancellationToken)
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Handle = NormalizeHandle(request.Handle),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsPublished = request.IsPublished
        };

        await _repository.AddCollectionAsync(collection, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return collection.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CollectionResponse?> UpdateCollectionAsync(Guid id, UpdateCollectionRequest request, CancellationToken cancellationToken)
    {
        var collection = await _repository.GetCollectionByIdAsync(id, cancellationToken);
        if (collection is null)
        {
            return null;
        }

        collection.Handle = NormalizeHandle(request.Handle);
        collection.Title = request.Title.Trim();
        collection.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        collection.IsPublished = request.IsPublished;
        await _repository.SaveChangesAsync(cancellationToken);
        return collection.ToResponse();
    }

    /// <inheritdoc />
    public async Task<bool> UnpublishCollectionAsync(Guid id, CancellationToken cancellationToken)
    {
        var collection = await _repository.GetCollectionByIdAsync(id, cancellationToken);
        if (collection is null)
        {
            return false;
        }

        collection.IsPublished = false;
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<CartResponse> CreateCartAsync(CreateCartRequest request, CancellationToken cancellationToken)
    {
        if (request.CustomerId is null && string.IsNullOrWhiteSpace(request.AnonymousKey))
        {
            throw new InvalidOperationException("A cart requires either a customer id or an anonymous key.");
        }

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            AnonymousKey = string.IsNullOrWhiteSpace(request.AnonymousKey) ? null : request.AnonymousKey.Trim(),
            Currency = NormalizeCurrency(request.Currency),
            Status = CartStatus.Active
        };

        await _repository.AddCartAsync(cart, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return cart.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CartResponse?> GetCartAsync(Guid id, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(id, cancellationToken);
        return cart?.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CartResponse?> UpsertCartLineAsync(Guid cartId, UpsertCartLineRequest request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(cartId, cancellationToken);
        if (cart is null)
        {
            return null;
        }

        if (!string.Equals(cart.Status, CartStatus.Active, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only active carts can be edited.");
        }

        var variant = await _repository.GetVariantAsync(request.ProductVariantId, cancellationToken)
            ?? throw new InvalidOperationException("Product variant was not found.");

        if (!variant.IsActive)
        {
            throw new InvalidOperationException("Product variant is not active.");
        }

        if (!string.Equals(cart.Currency, variant.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cart currency does not match product variant currency.");
        }

        var line = cart.Lines.FirstOrDefault(existing => existing.ProductVariantId == request.ProductVariantId);
        if (line is null)
        {
            line = new CartLine
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductVariantId = variant.Id
            };
            cart.Lines.Add(line);
        }

        line.Quantity = request.Quantity;
        line.UnitPriceAmount = variant.PriceAmount;
        line.Currency = variant.Currency;
        line.Sku = variant.Sku;
        line.Title = variant.Product is null ? variant.Title : $"{variant.Product.Title} - {variant.Title}";
        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return cart.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CartResponse?> RemoveCartLineAsync(Guid cartId, Guid lineId, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(cartId, cancellationToken);
        if (cart is null)
        {
            return null;
        }

        var line = cart.Lines.FirstOrDefault(existing => existing.Id == lineId);
        if (line is not null)
        {
            cart.Lines.Remove(line);
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return cart.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(request.CartId, cancellationToken)
            ?? throw new InvalidOperationException("Cart was not found.");

        if (cart.Lines.Count == 0)
        {
            throw new InvalidOperationException("Cart must contain at least one item before checkout.");
        }

        var total = cart.Lines.Sum(line => line.Quantity * line.UnitPriceAmount);
        var session = new CheckoutSession
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            CustomerId = request.CustomerId,
            Currency = cart.Currency,
            TotalAmount = total,
            ShippingAddressJson = request.ShippingAddressJson,
            BillingAddressJson = request.BillingAddressJson
        };

        cart.CustomerId ??= request.CustomerId;
        cart.Status = CartStatus.Checkout;
        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _repository.AddCheckoutSessionAsync(session, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return session.ToResponse();
    }

    /// <inheritdoc />
    public async Task<CheckoutSessionResponse?> GetCheckoutSessionAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await _repository.GetCheckoutSessionAsync(id, cancellationToken);
        return session?.ToResponse();
    }

    /// <inheritdoc />
    public async Task<StoreOrderResponse?> CompleteCheckoutAsync(Guid checkoutSessionId, CancellationToken cancellationToken)
    {
        var session = await _repository.GetCheckoutSessionAsync(checkoutSessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (!string.Equals(session.Status, CheckoutStatus.Open, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Checkout session is not open.");
        }

        if (session.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            session.Status = CheckoutStatus.Expired;
            await _repository.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Checkout session is expired.");
        }

        var cart = session.Cart ?? await _repository.GetCartAsync(session.CartId, cancellationToken)
            ?? throw new InvalidOperationException("Cart was not found.");

        var order = new StoreOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"MWS-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            CustomerId = session.CustomerId,
            CheckoutSessionId = session.Id,
            Status = StoreOrderStatus.PendingPayment,
            Currency = session.Currency,
            TotalAmount = session.TotalAmount,
            ShippingAddressJson = session.ShippingAddressJson,
            BillingAddressJson = session.BillingAddressJson
        };

        foreach (var cartLine in cart.Lines)
        {
            order.Lines.Add(new StoreOrderLine
            {
                Id = Guid.NewGuid(),
                StoreOrderId = order.Id,
                ProductVariantId = cartLine.ProductVariantId,
                Sku = cartLine.Sku,
                Title = cartLine.Title,
                Quantity = cartLine.Quantity,
                UnitPriceAmount = cartLine.UnitPriceAmount,
                Currency = cartLine.Currency
            });
        }

        session.Status = CheckoutStatus.Completed;
        cart.Status = CartStatus.Ordered;
        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _repository.AddStoreOrderAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return order.ToResponse();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StoreOrderResponse>> ListOrdersAsync(Guid? customerId, CancellationToken cancellationToken)
    {
        var orders = await _repository.ListStoreOrdersAsync(customerId, cancellationToken);
        return orders.Select(order => order.ToResponse()).ToList();
    }

    /// <inheritdoc />
    public async Task<StoreOrderResponse?> GetOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _repository.GetStoreOrderAsync(id, cancellationToken);
        return order?.ToResponse();
    }

    private static Product BuildProduct(CreateProductRequest request, Product product)
    {
        ApplyProductBasics(request, product);

        foreach (var variantRequest in request.Variants)
        {
            product.Variants.Add(CreateVariant(product.Id, variantRequest));
        }

        foreach (var mediaRequest in request.Media)
        {
            product.Media.Add(CreateMedia(product.Id, mediaRequest));
        }

        return product;
    }

    private static void ApplyProductBasics(CreateProductRequest request, Product product)
    {
        product.Handle = NormalizeHandle(request.Handle);
        product.Title = request.Title.Trim();
        product.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
        product.Summary = request.Summary.Trim();
        product.Description = request.Description.Trim();
        product.ProductType = request.ProductType.Trim();
        product.Status = NormalizeProductStatus(request.Status);
    }

    private void SyncVariants(Product product, IEnumerable<CreateProductVariantRequest> variantRequests)
    {
        var requests = variantRequests.ToList();
        var existingVariants = product.Variants.ToList();
        var requestedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matchedVariantIds = new HashSet<Guid>();
        var matchedVariants = new ProductVariant?[requests.Count];

        for (var index = 0; index < requests.Count; index++)
        {
            var variantRequest = requests[index];
            var sku = variantRequest.Sku.Trim();
            if (!requestedSkus.Add(sku))
            {
                throw new InvalidOperationException($"Duplicate variant SKU '{sku}'.");
            }

            var existing = existingVariants.FirstOrDefault(variant =>
                !matchedVariantIds.Contains(variant.Id) &&
                string.Equals(variant.Sku, sku, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                matchedVariants[index] = existing;
                matchedVariantIds.Add(existing.Id);
            }
        }

        for (var index = 0; index < requests.Count; index++)
        {
            var variant = matchedVariants[index];
            if (variant is null && index < existingVariants.Count && !matchedVariantIds.Contains(existingVariants[index].Id))
            {
                variant = existingVariants[index];
            }

            variant ??= existingVariants.FirstOrDefault(existing => !matchedVariantIds.Contains(existing.Id));

            if (variant is null)
            {
                product.Variants.Add(CreateVariant(product.Id, requests[index]));
                continue;
            }

            matchedVariantIds.Add(variant.Id);
            ApplyVariant(variant, requests[index]);
        }

        var variantsToRemove = existingVariants
            .Where(variant => !matchedVariantIds.Contains(variant.Id))
            .ToList();
        foreach (var variant in variantsToRemove)
        {
            product.Variants.Remove(variant);
        }

        _repository.RemoveProductVariants(variantsToRemove);
    }

    private static ProductVariant CreateVariant(Guid productId, CreateProductVariantRequest request)
    {
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId
        };

        ApplyVariant(variant, request);
        return variant;
    }

    private static void ApplyVariant(ProductVariant variant, CreateProductVariantRequest request)
    {
        variant.Sku = request.Sku.Trim();
        variant.Title = request.Title.Trim();
        variant.PriceAmount = decimal.Round(request.PriceAmount, 2, MidpointRounding.AwayFromZero);
        variant.Currency = NormalizeCurrency(request.Currency);
        variant.InventoryQuantity = request.InventoryQuantity;
        variant.IsActive = true;
        variant.OptionValuesJson = request.OptionValuesJson;
    }

    private void SyncMedia(Product product, IEnumerable<CreateProductMediaRequest> mediaRequests)
    {
        var requests = mediaRequests.ToList();
        var existingMedia = product.Media
            .OrderBy(media => media.SortOrder)
            .ThenBy(media => media.Id)
            .ToList();

        for (var index = 0; index < requests.Count; index++)
        {
            if (index < existingMedia.Count)
            {
                ApplyMedia(existingMedia[index], requests[index]);
                continue;
            }

            product.Media.Add(CreateMedia(product.Id, requests[index]));
        }

        var mediaToRemove = existingMedia
            .Skip(requests.Count)
            .ToList();
        foreach (var media in mediaToRemove)
        {
            product.Media.Remove(media);
        }

        _repository.RemoveProductMedia(mediaToRemove);
    }

    private static ProductMedia CreateMedia(Guid productId, CreateProductMediaRequest request)
    {
        var media = new ProductMedia
        {
            Id = Guid.NewGuid(),
            ProductId = productId
        };

        ApplyMedia(media, request);
        return media;
    }

    private static void ApplyMedia(ProductMedia media, CreateProductMediaRequest request)
    {
        media.Url = request.Url.Trim();
        media.AltText = string.IsNullOrWhiteSpace(request.AltText) ? null : request.AltText.Trim();
        media.SortOrder = request.SortOrder;
    }

    private async Task LinkCollectionsAsync(Product product, IEnumerable<string> handles, CancellationToken cancellationToken)
    {
        var sortOrder = 0;
        foreach (var rawHandle in handles.Where(handle => !string.IsNullOrWhiteSpace(handle)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var handle = NormalizeHandle(rawHandle);
            var collection = await _repository.GetCollectionByHandleAsync(handle, cancellationToken);
            if (collection is null)
            {
                continue;
            }

            product.ProductCollections.Add(new ProductCollection
            {
                ProductId = product.Id,
                CollectionId = collection.Id,
                Collection = collection,
                SortOrder = sortOrder++
            });
        }
    }

    private async Task SyncCollectionsAsync(Product product, IEnumerable<string> handles, CancellationToken cancellationToken)
    {
        var requestedHandles = handles
            .Where(handle => !string.IsNullOrWhiteSpace(handle))
            .Select(NormalizeHandle)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var requestedHandleSet = requestedHandles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var linksToRemove = product.ProductCollections
            .Where(link => link.Collection is null || !requestedHandleSet.Contains(link.Collection.Handle))
            .ToList();
        foreach (var link in linksToRemove)
        {
            product.ProductCollections.Remove(link);
        }

        _repository.RemoveProductCollections(linksToRemove);

        var existingHandles = product.ProductCollections
            .Where(link => link.Collection is not null)
            .Select(link => link.Collection!.Handle)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sortOrder = product.ProductCollections.Count;
        foreach (var handle in requestedHandles)
        {
            if (existingHandles.Contains(handle))
            {
                continue;
            }

            var collection = await _repository.GetCollectionByHandleAsync(handle, cancellationToken);
            if (collection is null)
            {
                continue;
            }

            product.ProductCollections.Add(new ProductCollection
            {
                ProductId = product.Id,
                CollectionId = collection.Id,
                Collection = collection,
                SortOrder = sortOrder++
            });
        }
    }

    private static string NormalizeHandle(string handle)
    {
        var normalized = handle.Trim().ToLowerInvariant().Replace(' ', '-');
        return normalized;
    }

    private static string NormalizeCurrency(string currency)
    {
        return currency.Trim().ToUpperInvariant();
    }

    private static string NormalizeProductStatus(string status)
    {
        return status.Trim().ToLowerInvariant() switch
        {
            "published" or "active" => ProductStatus.Published,
            "archived" => ProductStatus.Archived,
            _ => ProductStatus.Draft
        };
    }
}
