using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Interfaces;
using Maliev.CommerceService.Application.Services;
using Maliev.CommerceService.Domain.Entities;
using Moq;

namespace Maliev.CommerceService.Tests.Unit;

public sealed class CommerceServiceTests
{
    [Fact]
    public async Task CreateCartAsync_WithNoCustomerOrAnonymousKey_Throws()
    {
        var repository = new Mock<ICommerceRepository>();
        var service = new CommerceService.Application.Services.CommerceService(repository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCartAsync(new CreateCartRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task UpsertCartLineAsync_WithActiveVariant_AddsPriceSnapshot()
    {
        var product = new Product { Id = Guid.NewGuid(), Handle = "simmount", Title = "SimMount", Status = ProductStatus.Published };
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Sku = "SIM-001",
            Title = "Base",
            PriceAmount = 1250,
            Currency = "THB",
            InventoryQuantity = 10,
            IsActive = true
        };
        var cart = new Cart { Id = Guid.NewGuid(), AnonymousKey = "browser", Currency = "THB" };

        var repository = new Mock<ICommerceRepository>();
        repository.Setup(repo => repo.GetCartAsync(cart.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        repository.Setup(repo => repo.GetVariantAsync(variant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(variant);
        var service = new CommerceService.Application.Services.CommerceService(repository.Object);

        var result = await service.UpsertCartLineAsync(cart.Id, new UpsertCartLineRequest { ProductVariantId = variant.Id, Quantity = 2 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!.Lines);
        Assert.Equal(2500, result.TotalAmount);
        Assert.Equal("SimMount - Base", result.Lines[0].Title);
        repository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteCheckoutAsync_WithOpenSession_CreatesStoreOrder()
    {
        var cart = new Cart { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Currency = "THB", Status = CartStatus.Checkout };
        cart.Lines.Add(new CartLine
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductVariantId = Guid.NewGuid(),
            Quantity = 3,
            UnitPriceAmount = 100,
            Currency = "THB",
            Sku = "PART-001",
            Title = "Spare part"
        });
        var session = new CheckoutSession
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            Cart = cart,
            CustomerId = cart.CustomerId.Value,
            TotalAmount = 300,
            Currency = "THB",
            Status = CheckoutStatus.Open,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1)
        };

        StoreOrder? capturedOrder = null;
        var repository = new Mock<ICommerceRepository>();
        repository.Setup(repo => repo.GetCheckoutSessionAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        repository.Setup(repo => repo.AddStoreOrderAsync(It.IsAny<StoreOrder>(), It.IsAny<CancellationToken>()))
            .Callback<StoreOrder, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        var service = new CommerceService.Application.Services.CommerceService(repository.Object);

        var result = await service.CompleteCheckoutAsync(session.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(capturedOrder);
        Assert.Equal(StoreOrderStatus.PendingPayment, capturedOrder!.Status);
        Assert.Equal(CheckoutStatus.Completed, session.Status);
        Assert.Equal(CartStatus.Ordered, cart.Status);
        Assert.Equal(300, result!.TotalAmount);
    }
}
