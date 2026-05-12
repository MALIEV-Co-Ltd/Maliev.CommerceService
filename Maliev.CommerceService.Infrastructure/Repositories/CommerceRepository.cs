using Maliev.CommerceService.Application.Interfaces;
using Maliev.CommerceService.Domain.Entities;
using Maliev.CommerceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CommerceService.Infrastructure.Repositories;

/// <inheritdoc />
public sealed class CommerceRepository(CommerceDbContext dbContext) : ICommerceRepository
{
    private readonly CommerceDbContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> ListProductsAsync(string? query, string? collectionHandle, int page, int pageSize, bool includeDrafts, CancellationToken cancellationToken)
    {
        var products = _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Variants)
            .Include(product => product.Media)
            .Include(product => product.ProductCollections)
                .ThenInclude(link => link.Collection)
            .AsQueryable();

        if (!includeDrafts)
        {
            products = products.Where(product => product.Status == ProductStatus.Published);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim().ToLowerInvariant();
            products = products.Where(product =>
                product.Title.ToLower().Contains(normalized) ||
                product.Summary.ToLower().Contains(normalized) ||
                product.ProductType.ToLower().Contains(normalized));
        }

        if (!string.IsNullOrWhiteSpace(collectionHandle))
        {
            var normalizedHandle = collectionHandle.Trim().ToLowerInvariant();
            products = products.Where(product => product.ProductCollections.Any(link => link.Collection != null && link.Collection.Handle == normalizedHandle));
        }

        var totalCount = await products.CountAsync(cancellationToken);
        var items = await products
            .OrderBy(product => product.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductByHandleAsync(string handle, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Include(product => product.Variants)
            .Include(product => product.Media)
            .Include(product => product.ProductCollections)
                .ThenInclude(link => link.Collection)
            .FirstOrDefaultAsync(product => product.Handle == handle, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Include(product => product.Variants)
            .Include(product => product.Media)
            .Include(product => product.ProductCollections)
                .ThenInclude(link => link.Collection)
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Collection>> ListCollectionsAsync(bool includeUnpublished, CancellationToken cancellationToken)
    {
        var query = _dbContext.Collections.AsNoTracking().AsQueryable();
        if (!includeUnpublished)
        {
            query = query.Where(collection => collection.IsPublished);
        }

        return await query.OrderBy(collection => collection.Title).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Collection?> GetCollectionByHandleAsync(string handle, CancellationToken cancellationToken)
    {
        return await _dbContext.Collections.FirstOrDefaultAsync(collection => collection.Handle == handle, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Collection?> GetCollectionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Collections.FirstOrDefaultAsync(collection => collection.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCollectionAsync(Collection collection, CancellationToken cancellationToken)
    {
        await _dbContext.Collections.AddAsync(collection, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductVariant?> GetVariantAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.ProductVariants
            .Include(variant => variant.Product)
            .FirstOrDefaultAsync(variant => variant.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCartAsync(Cart cart, CancellationToken cancellationToken)
    {
        await _dbContext.Carts.AddAsync(cart, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Cart?> GetCartAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Carts
            .Include(cart => cart.Lines)
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCheckoutSessionAsync(CheckoutSession checkoutSession, CancellationToken cancellationToken)
    {
        await _dbContext.CheckoutSessions.AddAsync(checkoutSession, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CheckoutSession?> GetCheckoutSessionAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.CheckoutSessions
            .Include(session => session.Cart)
                .ThenInclude(cart => cart!.Lines)
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddStoreOrderAsync(StoreOrder order, CancellationToken cancellationToken)
    {
        await _dbContext.StoreOrders.AddAsync(order, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StoreOrder>> ListStoreOrdersAsync(Guid? customerId, CancellationToken cancellationToken)
    {
        var orders = _dbContext.StoreOrders
            .AsNoTracking()
            .Include(order => order.Lines)
            .AsQueryable();

        if (customerId.HasValue)
        {
            orders = orders.Where(order => order.CustomerId == customerId.Value);
        }

        return await orders.OrderByDescending(order => order.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<StoreOrder?> GetStoreOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.StoreOrders
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
