using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MadameCoco.Customer.API.DTOs;
using MadameCoco.Customer.API.Interfaces;
using MadameCoco.Customer.API.Services;
using MadameCoco.Shared.BaseEntities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace MadameCoco.Customer.Tests.UnitTests;


public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<API.Entities.Customer>> _mockCustomerRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IValidator<API.Entities.Customer>> _mockValidator;
    private readonly CustomerService _customerService;

 
    public CustomerServiceTests()
    {
        // Mock nesneleri oluştur
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCustomerRepository = new Mock<IRepository<API.Entities.Customer>>();
        _mockMapper = new Mock<IMapper>();
        _mockValidator = new Mock<IValidator<API.Entities.Customer>>();

        // UnitOfWork'ün CustomerRepository property'sini mock repository ile değiştir
        _mockUnitOfWork.Setup(x => x.CustomerRepository).Returns(_mockCustomerRepository.Object);

        // Test edilecek servisi oluştur (mock bağımlılıklar ile)
        _customerService = new CustomerService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockValidator.Object
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingCustomer_ReturnsSuccessResult()
    {
        // ==================== ARRANGE (Hazırlık) ====================
        // Test verilerini hazırla
        var customerId = Guid.NewGuid();
        var expectedCustomer = new API.Entities.Customer
        {
            Id = customerId,
            Name = "Ahmet Yılmaz",
            Email = "ahmet@test.com",
            Address = new Address
            {
                AddressLine = "Test Caddesi",
                City = "Istanbul",
                Country = "Turkey",
                CityCode = 34
            }
        };

        var expectedDto = new CustomerResponseDto(
            customerId,
            "Ahmet Yılmaz",
            "ahmet@test.com",
            new AddressDto("Test Caddesi", "Istanbul", "Turkey", 34),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        // Mock davranışlarını ayarla
        // Repository'den FindAsync çağrıldığında expectedCustomer'ı döndür
        _mockCustomerRepository
            .Setup(x => x.FindAsync(customerId))
            .ReturnsAsync(expectedCustomer);

        // Mapper çağrıldığında expectedDto'yu döndür
        _mockMapper
            .Setup(x => x.Map<CustomerResponseDto>(expectedCustomer))
            .Returns(expectedDto);

        // ==================== ACT (Eylem) ====================
        // Test edilecek metodu çalıştır
        var result = await _customerService.GetByIdAsync(customerId);

        // ==================== ASSERT (Doğrulama) ====================
        // Sonuçları kontrol et
        result.Should().NotBeNull("çünkü servis her zaman bir sonuç döndürmeli");
        result.IsSuccess.Should().BeTrue("çünkü müşteri bulundu");
        result.ResultObject.Should().NotBeNull("çünkü başarılı sonuçta veri olmalı");
        result.ResultObject!.Detail.Should().NotBeNull();
        result.ResultObject.Detail!.Name.Should().Be("Ahmet Yılmaz");

        // Mock'ların doğru çağrıldığını kontrol et
        _mockCustomerRepository.Verify(
            x => x.FindAsync(customerId),
            Times.Once, // Tam olarak 1 kez çağrılmalı
            "çünkü servis repository'den müşteriyi getirmeli"
        );
    }

    /// <summary>
    /// TEST 2: Olmayan bir müşteri ID'si ile çağrıldığında hata dönmeli
    /// 
    /// SENARYO: Sistemde olmayan bir müşteri ID'si ile arama yapıyoruz
    /// BEKLENTİ: IsSuccess = false, hata mesajı olmalı
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistingCustomer_ReturnsErrorResult()
    {
        // ==================== ARRANGE ====================
        var customerId = Guid.NewGuid();

        // Repository null döndürsün (müşteri bulunamadı)
        _mockCustomerRepository
            .Setup(x => x.FindAsync(customerId))
            .ReturnsAsync((API.Entities.Customer?)null);

        // ==================== ACT ====================
        var result = await _customerService.GetByIdAsync(customerId);

        // ==================== ASSERT ====================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("çünkü müşteri bulunamadı");
        result.Message.Should().Contain("bulunamadı", "çünkü hata mesajı açıklayıcı olmalı");

        _mockCustomerRepository.Verify(x => x.FindAsync(customerId), Times.Once);
    }

    #endregion

    #region ValidateAsync Tests

    /// <summary>
    /// TEST 3: Var olan müşteri için validate true dönmeli
    /// 
    /// SENARYO: Order servisi müşterinin var olup olmadığını kontrol ediyor
    /// BEKLENTİ: true dönmeli
    /// </summary>
    [Fact]
    public async Task ValidateAsync_ExistingCustomer_ReturnsTrue()
    {
        // ==================== ARRANGE ====================
        var customerId = Guid.NewGuid();

        // MockQueryable kullanarak async sorguları destekleyen mock listesi oluştur
        var customers = new List<API.Entities.Customer>
        {
            new() { Id = customerId, Name = "Test", Email = "test@test.com" }
        }.AsQueryable().BuildMockDbSet();

        _mockCustomerRepository
            .Setup(x => x.GetAll())
            .Returns(customers.Object);

        // ==================== ACT ====================
        var result = await _customerService.ValidateAsync(customerId);

        // ==================== ASSERT ====================
        result.Should().BeTrue("çünkü müşteri sistemde mevcut");
    }

    /// <summary>
    /// TEST 4: Olmayan müşteri için validate false dönmeli
    /// </summary>
    [Fact]
    public async Task ValidateAsync_NonExistingCustomer_ReturnsFalse()
    {
        // ==================== ARRANGE ====================
        var customerId = Guid.NewGuid();

        // Boş liste
        var customers = new List<API.Entities.Customer>()
            .AsQueryable()
            .BuildMockDbSet();

        _mockCustomerRepository
            .Setup(x => x.GetAll())
            .Returns(customers.Object);

        // ==================== ACT ====================
        var result = await _customerService.ValidateAsync(customerId);

        // ==================== ASSERT ====================
        result.Should().BeFalse("çünkü müşteri sistemde yok");
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// TEST 5: Var olan müşteri silinebilmeli
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingCustomer_ReturnsSuccess()
    {
        // ==================== ARRANGE ====================
        var customerId = Guid.NewGuid();
        var customer = new API.Entities.Customer
        {
            Id = customerId,
            Name = "Silinecek Müşteri",
            Email = "delete@test.com"
        };

        _mockCustomerRepository
            .Setup(x => x.FindAsync(customerId))
            .ReturnsAsync(customer);

        // ==================== ACT ====================
        var result = await _customerService.DeleteAsync(customerId);

        // ==================== ASSERT ====================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("silindi");

        // Delete metodunun çağrıldığını doğrula
        _mockCustomerRepository.Verify(
            x => x.Delete(customer),
            Times.Once,
            "çünkü müşteri silinmeli"
        );
    }

    /// <summary>
    /// TEST 6: Olmayan müşteri silinmeye çalışıldığında hata dönmeli
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistingCustomer_ReturnsError()
    {
        // ==================== ARRANGE ====================
        var customerId = Guid.NewGuid();

        _mockCustomerRepository
            .Setup(x => x.FindAsync(customerId))
            .ReturnsAsync((API.Entities.Customer?)null);

        // ==================== ACT ====================
        var result = await _customerService.DeleteAsync(customerId);

        // ==================== ASSERT ====================
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("bulunamadı");

        // Delete metodunun çağrılMADIĞINI doğrula
        _mockCustomerRepository.Verify(
            x => x.Delete(It.IsAny<API.Entities.Customer>()),
            Times.Never,
            "çünkü müşteri bulunamadığı için silme işlemi yapılmamalı"
        );
    }

    #endregion

}
