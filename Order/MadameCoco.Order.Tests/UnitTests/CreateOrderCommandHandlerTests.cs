using FluentAssertions;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.DTOs;
using MadameCoco.Order.API.Features.Order.Commands.CreateOrder;
using MadameCoco.Order.API.Features.Order.Commands.OrderCommands;
using MadameCoco.Order.API.Interfaces;
using MadameCoco.Shared.BaseEntities;
using MadameCoco.Shared.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MadameCoco.Order.Tests.UnitTests;

/// <summary>
/// CreateOrderCommandHandler için Unit Testler
/// 
/// ÖNEMLİ KAVRAMLAR:
/// - CQRS Pattern: Command Query Responsibility Segregation
///   * Commands: Veri değiştiren işlemler (Create, Update, Delete)
///   * Queries: Veri okuyan işlemler (Get, List)
/// - MediatR: CQRS pattern'i implement etmek için kullanılan kütüphane
/// - Handler: Command/Query'leri işleyen sınıflar
/// 
/// Bu test, CreateOrderCommandHandler'ı test eder
/// Handler, sipariş oluşturma business logic'ini içerir
/// </summary>
public class CreateOrderCommandHandlerTests : IDisposable
{
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ICustomerClient> _mockCustomerClient;
    private readonly OrderDbContext _dbContext;
    private readonly CreateOrderCommandHandler _handler;

    /// <summary>
    /// Constructor - Her test için temiz bir ortam hazırlar
    /// 
    /// NOT: OrderDbContext için in-memory database kullanıyoruz
    /// Bu sayede gerçek database'e bağlanmadan test yapabiliriz
    /// </summary>
    public CreateOrderCommandHandlerTests()
    {
        // In-memory database oluştur
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Her test için yeni DB
            .Options;

        _dbContext = new OrderDbContext(options);

        // Mock nesneleri oluştur
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockCustomerClient = new Mock<ICustomerClient>();

        // Handler'ı oluştur (test edilecek sınıf)
        _handler = new CreateOrderCommandHandler(
            _mockPublishEndpoint.Object,
            _dbContext,
            _mockCustomerClient.Object
        );
    }

    #region Successful Order Creation Tests

    /// <summary>
    /// TEST 1: Geçerli sipariş oluşturma başarılı olmalı
    /// 
    /// SENARYO: Var olan bir müşteri için sipariş oluştur
    /// BEKLENTİ: 
    /// - IsSuccess = true
    /// - Order ID dönmeli
    /// - Database'e kayıt atılmalı
    /// - Event publish edilmeli
    /// </summary>
    [Fact]
    public async Task Handle_ValidOrder_ReturnsSuccessWithOrderId()
    {
        // ==================== ARRANGE ====================
        var customerId = Guid.NewGuid();

        // Customer servisi müşterinin var olduğunu söylesin
        _mockCustomerClient
            .Setup(x => x.ValidateCustomerExistsAsync(customerId))
            .ReturnsAsync(true);

        // Event publish işlemini mock'la
        _mockPublishEndpoint
            .Setup(x => x.Publish(
                It.IsAny<OrderCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Sipariş command'ı oluştur (record constructor syntax)
        var command = new CreateOrderCommand(
            CustomerId: customerId,
            ShippingAddress: new Address
            {
                AddressLine = "Test Caddesi No:123",
                City = "Istanbul",
                Country = "Turkey",
                CityCode = 34
            },
            Items: new List<OrderItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Ürün 1",
                    Quantity = 2,
                    Price = 100.50m,
                    ImageUrl = "https://example.com/image1.jpg"
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Ürün 2",
                    Quantity = 1,
                    Price = 50.25m,
                    ImageUrl = "https://example.com/image2.jpg"
                }
            }
        );

        // ==================== ACT ====================
        var result = await _handler.Handle(command, CancellationToken.None);

        // ==================== ASSERT ====================
        
        // 1. Sonuç kontrolü
        result.Should().NotBeNull("handler her zaman sonuç dönmeli");
        result.IsSuccess.Should().BeTrue("geçerli bir sipariş oluşturduk");
        result.ResultObject.Should().NotBeEmpty("sipariş ID'si dönmeli");

        // 2. Database kontrolü
        var savedOrder = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result.ResultObject);

        savedOrder.Should().NotBeNull("sipariş database'e kaydedilmeli");
        savedOrder!.CustomerId.Should().Be(customerId);
        savedOrder.Items.Should().HaveCount(2, "2 ürün ekledik");
        savedOrder.Status.Should().Be(API.Entities.Enums.OrderStatus.Pending);

        // 3. Event publish kontrolü
        _mockPublishEndpoint.Verify(
            x => x.Publish(
                It.Is<OrderCreatedEvent>(e =>
                    e.OrderId == result.ResultObject &&
                    e.CustomerId == customerId &&
                    e.Quantity == command.Items.Sum(i => i.Quantity) &&
                    e.TotalPrice == savedOrder.TotalPrice
                ),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "sipariş oluşturulduğunda doğru bilgilerle event fırlatılmalı"
        );

        // 4. Customer validation kontrolü
        _mockCustomerClient.Verify(
            x => x.ValidateCustomerExistsAsync(customerId),
            Times.Once,
            "müşteri kontrolü yapılmalı"
        );
    }

    #endregion

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
