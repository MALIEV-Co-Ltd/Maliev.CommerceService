using Maliev.CommerceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CommerceService.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for storefront commerce.
/// </summary>
public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options) : DbContext(options)
{
    /// <summary>Gets products.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Gets product variants.</summary>
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    /// <summary>Gets product media.</summary>
    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();

    /// <summary>Gets collections.</summary>
    public DbSet<Collection> Collections => Set<Collection>();

    /// <summary>Gets product collection links.</summary>
    public DbSet<ProductCollection> ProductCollections => Set<ProductCollection>();

    /// <summary>Gets carts.</summary>
    public DbSet<Cart> Carts => Set<Cart>();

    /// <summary>Gets cart lines.</summary>
    public DbSet<CartLine> CartLines => Set<CartLine>();

    /// <summary>Gets checkout sessions.</summary>
    public DbSet<CheckoutSession> CheckoutSessions => Set<CheckoutSession>();

    /// <summary>Gets store orders.</summary>
    public DbSet<StoreOrder> StoreOrders => Set<StoreOrder>();

    /// <summary>Gets store order lines.</summary>
    public DbSet<StoreOrderLine> StoreOrderLines => Set<StoreOrderLine>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.HasIndex(product => product.Handle).IsUnique();
            entity.Property(product => product.Handle).HasMaxLength(160).IsRequired();
            entity.Property(product => product.Title).HasMaxLength(240).IsRequired();
            entity.Property(product => product.Brand).HasMaxLength(120);
            entity.Property(product => product.Summary).HasMaxLength(500);
            entity.Property(product => product.ProductType).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Status).HasMaxLength(40).IsRequired();
            entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(variant => variant.Id);
            entity.HasIndex(variant => variant.Sku).IsUnique();
            entity.Property(variant => variant.Sku).HasMaxLength(120).IsRequired();
            entity.Property(variant => variant.Title).HasMaxLength(240).IsRequired();
            entity.Property(variant => variant.Currency).HasMaxLength(3).IsRequired();
            entity.Property(variant => variant.PriceAmount).HasPrecision(18, 2);
            entity.HasOne(variant => variant.Product)
                .WithMany(product => product.Variants)
                .HasForeignKey(variant => variant.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductMedia>(entity =>
        {
            entity.HasKey(media => media.Id);
            entity.Property(media => media.Url).HasMaxLength(1000).IsRequired();
            entity.Property(media => media.AltText).HasMaxLength(240);
            entity.HasOne(media => media.Product)
                .WithMany(product => product.Media)
                .HasForeignKey(media => media.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(collection => collection.Id);
            entity.HasIndex(collection => collection.Handle).IsUnique();
            entity.Property(collection => collection.Handle).HasMaxLength(160).IsRequired();
            entity.Property(collection => collection.Title).HasMaxLength(200).IsRequired();
            entity.Property(collection => collection.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<ProductCollection>(entity =>
        {
            entity.HasKey(link => new { link.ProductId, link.CollectionId });
            entity.HasOne(link => link.Product)
                .WithMany(product => product.ProductCollections)
                .HasForeignKey(link => link.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(link => link.Collection)
                .WithMany(collection => collection.ProductCollections)
                .HasForeignKey(link => link.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(cart => cart.Id);
            entity.HasIndex(cart => cart.CustomerId);
            entity.Property(cart => cart.AnonymousKey).HasMaxLength(160);
            entity.Property(cart => cart.Status).HasMaxLength(40).IsRequired();
            entity.Property(cart => cart.Currency).HasMaxLength(3).IsRequired();
            entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        });

        modelBuilder.Entity<CartLine>(entity =>
        {
            entity.HasKey(line => line.Id);
            entity.HasIndex(line => new { line.CartId, line.ProductVariantId }).IsUnique();
            entity.Property(line => line.UnitPriceAmount).HasPrecision(18, 2);
            entity.Property(line => line.Currency).HasMaxLength(3).IsRequired();
            entity.Property(line => line.Sku).HasMaxLength(120).IsRequired();
            entity.Property(line => line.Title).HasMaxLength(300).IsRequired();
            entity.HasOne(line => line.Cart)
                .WithMany(cart => cart.Lines)
                .HasForeignKey(line => line.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(line => line.ProductVariant)
                .WithMany()
                .HasForeignKey(line => line.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckoutSession>(entity =>
        {
            entity.HasKey(session => session.Id);
            entity.HasIndex(session => session.CustomerId);
            entity.Property(session => session.Status).HasMaxLength(40).IsRequired();
            entity.Property(session => session.Currency).HasMaxLength(3).IsRequired();
            entity.Property(session => session.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(session => session.Cart)
                .WithMany()
                .HasForeignKey(session => session.CartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StoreOrder>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.HasIndex(order => order.CustomerId);
            entity.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.Status).HasMaxLength(40).IsRequired();
            entity.Property(order => order.Currency).HasMaxLength(3).IsRequired();
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(order => order.CheckoutSession)
                .WithMany()
                .HasForeignKey(order => order.CheckoutSessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StoreOrderLine>(entity =>
        {
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Sku).HasMaxLength(120).IsRequired();
            entity.Property(line => line.Title).HasMaxLength(300).IsRequired();
            entity.Property(line => line.Currency).HasMaxLength(3).IsRequired();
            entity.Property(line => line.UnitPriceAmount).HasPrecision(18, 2);
            entity.HasOne(line => line.StoreOrder)
                .WithMany(order => order.Lines)
                .HasForeignKey(line => line.StoreOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
