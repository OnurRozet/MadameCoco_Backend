using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using MadameCoco.Order.API.DTOs;
using MadameCoco.Order.API.Features.Order.Commands.OrderCommands;
using MadameCoco.Shared.BaseEntities;
using System.Net.Http.Json;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Testing;
using MassTransit;

namespace MadameCoco.Order.Tests.IntegrationTests;

/// <summary>
/// OrdersController için Integration Testler
/// 
/// ÖNEMLİ KAVRAMLAR:
/// - Integration Test: Birden fazla bileşenin birlikte çalışmasını test eder
/// - WebApplicationFactory: Test için geçici web sunucusu
/// - Microservice Testing: Diğer servislere bağımlılıkları test etme
/// 
/// NOT: Order servisi Customer servisine bağımlı
/// Test ortamında Customer servisini mock'lamak gerekebilir
/// </summary>
public class OrdersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        Environment.SetEnvironmentVariable("InMemoryDbName", "OrderIntegrationTestDb");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextTypesToClear = new[]
                {
                    typeof(DbContextOptions<OrderDbContext>),
                    typeof(OrderDbContext)
                };

                foreach (var serviceType in dbContextTypesToClear)
                {
                    var descriptors = services.Where(d => d.ServiceType == serviceType).ToList();
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }
                }

                services.AddDbContext<OrderDbContext>(options =>
                {
                    options.UseInMemoryDatabase("OrderIntegrationTestDb");
                });

                var customerClientDescriptors = services.Where(d => d.ServiceType == typeof(ICustomerClient)).ToList();
                foreach (var descriptor in customerClientDescriptors)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton<ICustomerClient, FakeCustomerClient>();

                var massTransitDescriptors = services.Where(
                    d => d.ServiceType?.FullName?.Contains("MassTransit") == true ||
                         d.ServiceType?.FullName?.Contains("BusControl") == true
                ).ToList();

                foreach (var descriptor in massTransitDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddMassTransitTestHarness();

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                db.Database.EnsureCreated();

            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region GET /api/orders Tests

    /// <summary>
    /// TEST 1: GET /api/orders endpoint'i çalışmalı
    /// 
    /// SENARYO: Tüm siparişleri listele
    /// BEKLENTİ: HTTP 200 OK
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // ==================== ACT ====================
        var response = await _client.GetAsync("/api/orders");

        // ==================== ASSERT ====================
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "endpoint çalışıyor olmalı");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// TEST 2: GET /api/orders liste formatında veri dönmeli
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsListOfOrders()
    {
        await CreateSampleOrderAsync();

        var response = await _client.GetAsync("/api/orders");

        response.IsSuccessStatusCode.Should().BeTrue();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var list = body.GetProperty("resultObject").GetProperty("searchResult");
        list.ValueKind.Should().Be(JsonValueKind.Array);
    }

    #endregion

    #region GET /api/orders/{id} Tests

    /// <summary>
    /// TEST 3: Olmayan sipariş ID'si ile 404 dönmeli
    /// 
    /// SENARYO: Sistemde olmayan bir sipariş ara
    /// BEKLENTİ: HTTP 404 Not Found
    /// </summary>
    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // ==================== ARRANGE ====================
        var nonExistingId = Guid.NewGuid();

        // ==================== ACT ====================
        var response = await _client.GetAsync($"/api/orders/{nonExistingId}");

        // ==================== ASSERT ====================
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "bu ID'de sipariş yok");
    }

    #endregion

    #region POST /api/orders Tests

    /// <summary>
    /// TEST 4: Geçerli sipariş oluşturma başarılı olmalı
    /// </summary>
    [Fact]
    public async Task Create_ValidOrder_ReturnsSuccessAndId()
    {
        var response = await _client.PostAsync("/api/orders", BuildOrderContent());

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        body.GetProperty("resultObject").GetGuid().Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// TEST 5: Oluşturulan sipariş GET ile dönmeli
    /// </summary>
    [Fact]
    public async Task GetById_AfterCreate_ReturnsOrder()
    {
        var orderId = await CreateSampleOrderAsync();

        var response = await _client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var detail = body.GetProperty("resultObject").GetProperty("detail");
        detail.GetProperty("id").GetGuid().Should().Be(orderId);
    }

    #endregion

    private async Task<Guid> CreateSampleOrderAsync()
    {
        var response = await _client.PostAsync("/api/orders", BuildOrderContent());
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("resultObject").GetGuid();
    }

    private static StringContent BuildOrderContent()
    {
        var createCommand = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            ShippingAddress: new Address
            {
                AddressLine = "Test Cad. No:1",
                City = "Istanbul",
                Country = "Turkey",
                CityCode = 34
            },
            Items: new List<OrderItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Integration Test Ürün",
                    Quantity = 2,
                    Price = 150.75m,
                    ImageUrl = "https://example.com/product.jpg"
                }
            }
        );

        return new StringContent(
            JsonSerializer.Serialize(createCommand),
            Encoding.UTF8,
            "application/json"
        );
    }

    private class FakeCustomerClient : ICustomerClient
    {
        public Task<bool> ValidateCustomerExistsAsync(Guid customerId) => Task.FromResult(true);
    }
}
