using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Application.UseCases.Sales;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Sales;
using Nexus.Domain.Entities.Security;

namespace Nexus.Tests.Sales;

public class DeliveryServiceTests
{
    private readonly Mock<IDeliveryRepository> _mockDeliveryRepo;
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IKardexEntryService> _mockKardexService;
    private readonly Mock<IValidator<CreateDeliveryDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateDeliveryDto>> _mockUpdateValidator;
    private readonly Mock<IValidator<DeliverySearchRequest>> _mockSearchValidator;
    private readonly Mock<ILogger<DeliveryService>> _mockLogger;
    private readonly DeliveryService _sut;

    public DeliveryServiceTests()
    {
        _mockDeliveryRepo = new Mock<IDeliveryRepository>();
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockKardexService = new Mock<IKardexEntryService>();
        _mockCreateValidator = new Mock<IValidator<CreateDeliveryDto>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateDeliveryDto>>();
        _mockSearchValidator = new Mock<IValidator<DeliverySearchRequest>>();
        _mockLogger = new Mock<ILogger<DeliveryService>>();

        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateDeliveryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateDeliveryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockSearchValidator
            .Setup(v => v.ValidateAsync(It.IsAny<DeliverySearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new DeliveryService(
            _mockDeliveryRepo.Object,
            _mockOrderRepo.Object,
            _mockUserRepo.Object,
            _mockKardexService.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockSearchValidator.Object,
            _mockLogger.Object);
    }

    private static Delivery CreateDelivery(long id = 1, long companyId = 100, long orderId = 10,
        long userId = 5, string status = "InTransit")
    {
        return new Delivery
        {
            Id = id,
            CompanyId = companyId,
            OrderId = orderId,
            UserId = userId,
            Status = status
        };
    }

    private static User CreateUser(long id = 5)
    {
        return new User { Id = id, Username = "testuser", PasswordHash = "hash", FullName = "Test User" };
    }

    private static Order CreateOrderWithDetails(long id = 10, long companyId = 100, long? warehouseId = 1)
    {
        return new Order
        {
            Id = id,
            CompanyId = companyId,
            CustomerId = 1,
            UserId = 5,
            WarehouseId = warehouseId,
            OrderType = "Sale",
            Status = "Pending",
            TotalAmount = 110m,
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    Id = 100,
                    OrderId = id,
                    SkuId = 10,
                    Quantity = 5,
                    UnitPrice = 10m,
                    Subtotal = 50m
                },
                new()
                {
                    Id = 101,
                    OrderId = id,
                    SkuId = 20,
                    Quantity = 3,
                    UnitPrice = 20m,
                    Subtotal = 60m
                }
            }
        };
    }

    #region UpdateAsync - Delivery to Delivered

    [Fact]
    public async Task UpdateAsync_TransitioningToDelivered_AddsStockAndCreatesKardexEntries()
    {
        // Arrange
        const long companyId = 100;
        const long deliveryId = 1;
        const long orderId = 10;
        const long userId = 5;
        const long warehouseId = 1;

        var delivery = CreateDelivery(deliveryId, companyId, orderId, userId, "InTransit");
        var order = CreateOrderWithDetails(orderId, companyId, warehouseId);

        _mockDeliveryRepo.Setup(r => r.GetByIdAsync(deliveryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        _mockOrderRepo.Setup(r => r.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var dto = new UpdateDeliveryDto("Delivered", null, null, null, null);

        // Act
        var result = await _sut.UpdateAsync(deliveryId, dto, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Delivered", result.Data.Status);

        _mockKardexService.Verify(s => s.CreateEntryAsync(
                companyId, warehouseId, 10, userId, "Purchase", 5, "Delivery", "1", null, null, null,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mockKardexService.Verify(s => s.CreateEntryAsync(
                companyId, warehouseId, 20, userId, "Purchase", 3, "Delivery", "1", null, null, null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AlreadyDelivered_ReturnsValidationError()
    {
        // Arrange
        const long companyId = 100;
        const long deliveryId = 1;

        var delivery = CreateDelivery(deliveryId, companyId, 10, 5, "Delivered");

        _mockDeliveryRepo.Setup(r => r.GetByIdAsync(deliveryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var dto = new UpdateDeliveryDto("Delivered", null, null, null, null);

        // Act
        var result = await _sut.UpdateAsync(deliveryId, dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Delivery is already marked as delivered", result.Message);

        _mockKardexService.Verify(s => s.CreateEntryAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_TransitioningToInTransit_DoesNotCreateKardexEntries()
    {
        // Arrange
        const long companyId = 100;
        const long deliveryId = 1;

        var delivery = CreateDelivery(deliveryId, companyId, 10, 5, "Pending");

        _mockDeliveryRepo.Setup(r => r.GetByIdAsync(deliveryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var dto = new UpdateDeliveryDto("InTransit", null, null, null, null);

        // Act
        var result = await _sut.UpdateAsync(deliveryId, dto, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("InTransit", result.Data.Status);

        _mockKardexService.Verify(s => s.CreateEntryAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenDeliveryNotFound_ReturnsNotFound()
    {
        // Arrange
        const long companyId = 100;
        const long deliveryId = 999;

        _mockDeliveryRepo.Setup(r => r.GetByIdAsync(deliveryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var dto = new UpdateDeliveryDto("Delivered", null, null, null, null);

        // Act
        var result = await _sut.UpdateAsync(deliveryId, dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Delivery not found", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_WhenCompanyMismatch_ReturnsNotFound()
    {
        // Arrange
        const long deliveryId = 1;
        var delivery = CreateDelivery(deliveryId, 100, 10, 5, "Pending");

        _mockDeliveryRepo.Setup(r => r.GetByIdAsync(deliveryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var dto = new UpdateDeliveryDto("Delivered", null, null, null, null);

        // Act
        var result = await _sut.UpdateAsync(deliveryId, dto, 200);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Delivery not found", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidStatus_ReturnsValidationError()
    {
        // Arrange
        const long companyId = 100;
        const long deliveryId = 1;

        var delivery = CreateDelivery(deliveryId, companyId, 10, 5, "Pending");

        _mockDeliveryRepo.Setup(r => r.GetByIdAsync(deliveryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var dto = new UpdateDeliveryDto("InvalidStatus", null, null, null, null);

        // Act
        var result = await _sut.UpdateAsync(deliveryId, dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Status", result.Message);
    }

    #endregion
}
