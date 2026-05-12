using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Domain.Entities;

namespace Maliev.CommerceService.Application.Services;

/// <summary>
/// Maps commerce domain entities to DTOs.
/// </summary>
public static class CommerceMapper
{
    /// <summary>Maps a product to a response.</summary>
    public static ProductResponse ToResponse(this Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Handle,
            product.Title,
            product.Brand,
            product.Summary,
            product.Description,
            product.ProductType,
            product.Status,
            product.Variants.OrderBy(v => v.Title).Select(v => v.ToResponse()).ToList(),
            product.Media.OrderBy(m => m.SortOrder).Select(m => new ProductMediaResponse(m.Id, m.Url, m.AltText, m.SortOrder)).ToList(),
            product.ProductCollections
                .Where(pc => pc.Collection is not null)
                .OrderBy(pc => pc.SortOrder)
                .Select(pc => new CollectionSummaryResponse(pc.Collection!.Id, pc.Collection.Handle, pc.Collection.Title))
                .ToList());
    }

    /// <summary>Maps a product to a summary response.</summary>
    public static ProductSummaryResponse ToSummaryResponse(this Product product)
    {
        var activeVariants = product.Variants.Where(v => v.IsActive).ToList();
        var firstVariant = activeVariants.OrderBy(v => v.PriceAmount).FirstOrDefault();
        return new ProductSummaryResponse(
            product.Id,
            product.Handle,
            product.Title,
            product.Brand,
            product.Summary,
            product.ProductType,
            product.Status,
            firstVariant?.PriceAmount ?? 0,
            firstVariant?.Currency ?? "THB",
            product.Media.OrderBy(m => m.SortOrder).FirstOrDefault()?.Url);
    }

    /// <summary>Maps a variant to response.</summary>
    public static ProductVariantResponse ToResponse(this ProductVariant variant)
    {
        return new ProductVariantResponse(variant.Id, variant.Sku, variant.Title, variant.PriceAmount, variant.Currency, variant.InventoryQuantity, variant.IsActive, variant.OptionValuesJson);
    }

    /// <summary>Maps a collection to response.</summary>
    public static CollectionResponse ToResponse(this Collection collection)
    {
        return new CollectionResponse(collection.Id, collection.Handle, collection.Title, collection.Description, collection.IsPublished);
    }

    /// <summary>Maps a cart to response.</summary>
    public static CartResponse ToResponse(this Cart cart)
    {
        var lines = cart.Lines.OrderBy(line => line.Title).Select(line => new CartLineResponse(
            line.Id,
            line.ProductVariantId,
            line.Sku,
            line.Title,
            line.Quantity,
            line.UnitPriceAmount,
            line.Currency,
            line.UnitPriceAmount * line.Quantity)).ToList();

        return new CartResponse(cart.Id, cart.CustomerId, cart.AnonymousKey, cart.Status, cart.Currency, lines, lines.Sum(line => line.LineTotal));
    }

    /// <summary>Maps a checkout session to response.</summary>
    public static CheckoutSessionResponse ToResponse(this CheckoutSession session)
    {
        return new CheckoutSessionResponse(session.Id, session.CartId, session.CustomerId, session.Status, session.TotalAmount, session.Currency, session.ExpiresAtUtc);
    }

    /// <summary>Maps a store order to response.</summary>
    public static StoreOrderResponse ToResponse(this StoreOrder order)
    {
        var lines = order.Lines.OrderBy(line => line.Title).Select(line => new StoreOrderLineResponse(
            line.Id,
            line.ProductVariantId,
            line.Sku,
            line.Title,
            line.Quantity,
            line.UnitPriceAmount,
            line.Currency,
            line.UnitPriceAmount * line.Quantity)).ToList();

        return new StoreOrderResponse(order.Id, order.OrderNumber, order.CustomerId, order.Status, order.TotalAmount, order.Currency, lines, order.CreatedAtUtc);
    }
}
