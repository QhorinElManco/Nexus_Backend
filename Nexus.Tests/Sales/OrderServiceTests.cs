using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Application.UseCases.Sales;
using Nexus.Domain.Entities.Customers;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Sales;
using Nexus.Domain.Entities.Security;

namespace Nexus.Tests.Sales;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<ICustomerRepository> _mockCustomerRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ISmartInventoryRepository> _mockInventoryRepo;
    private readonly Mock<IKardexEntryService> _mockKardexService;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
        var mockOrderDetailRepo = new Mock<IOrderDetailRepository>();
        _mockCustomerRepo = new Mock<ICustomerRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockInventoryRepo = new Mock<ISmartInventoryRepository>();
        _mockKardexService = new Mock<IKardexEntryService>();
        var mockCreateValidator = new Mock<IValidator<CreateOrderDto>>();
        var mockUpdateValidator = new Mock<IValidator<UpdateOrderDto>>();
        var mockSearchValidator = new Mock<IValidator<OrderSearchRequest>>();
        var mockLogger = new Mock<ILogger<OrderService>>();

        mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateOrderDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateOrderDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        mockSearchValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new OrderService(
            _mockOrderRepo.Object,
            mockOrderDetailRepo.Object,
            _mockCustomerRepo.Object,
            _mockUserRepo.Object,
            _mockInventoryRepo.Object,
            _mockKardexService.Object,
            mockCreateValidator.Object,
            mockUpdateValidator.Object,
            mockSearchValidator.Object,
            mockLogger.Object);
    }

    private static Customer CreateCustomer(long id = 10, long companyId = 100)
    {
        return new Customer
        {
            Id = id,
            CompanyId = companyId,
            Name = "Test Customer",
            TaxId = "12345678",
            Status = "Active"
        };
    }

    private static User CreateUser(long id = 5)
    {
        return new User { Id = id, Username = "testuser", PasswordHash = "hash", FullName = "Test User" };
    }

    private static SmartInventory CreateInventory(long warehouseId, long skuId, int currentQuantity)
    {
        return new SmartInventory
        {
            Id = 1,
            WarehouseId = warehouseId,
            SkuId = skuId,
            SupplierId = 1,
            LeadTimeDays = 7,
            ReorderPoint = 10,
            TargetStock = 100,
            CoverageDays = 30,
            CurrentQuantity = currentQuantity
        };
    }

    private static Order CreateOrder(long id = 1, long companyId = 100, long customerId = 10,
        long userId = 5, long? warehouseId = 1, string orderType = "Sale", string status = "Pending",
        decimal totalAmount = 0m)
    {
        return new Order
        {
            Id = id,
            CompanyId = companyId,
            CustomerId = customerId,
            UserId = userId,
            WarehouseId = warehouseId,
            OrderType = orderType,
            Status = status,
            TotalAmount = totalAmount,
            OrderDetails = new List<OrderDetail>()
        };
    }

    #region CreateAsync - Stock Validation for Sale Orders

    [Fact]
    public async Task CreateAsync_WithSaleOrderAndSufficientStock_CreatesOrderAndKardexEntries()
    {
        // Arrange
        const long companyId = 100;
        const long userId = 5;
        const long warehouseId = 1;

        var dto = new CreateOrderDto(
            10,
            "Sale",
            null,
            null,
            warehouseId,
            new List<CreateOrderDetailDto> { new(100, 5, 10m), new(200, 3, 20m) });

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(companyId: companyId));
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(userId));

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInventory(warehouseId, 100, 50));
        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, 200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInventory(warehouseId, 200, 30));

        var createdOrder = CreateOrder(1, companyId, 10, userId,
            warehouseId, "Sale", totalAmount: 110m);

        _mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);
        _mockOrderRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _sut.CreateAsync(dto, companyId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(110m, result.Data.TotalAmount);

        _mockKardexService.Verify(s => s.CreateEntryAsync(
                companyId, warehouseId, 100, userId, "Sale", 5, "Order", "1", null, null, null,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mockKardexService.Verify(s => s.CreateEntryAsync(
                companyId, warehouseId, 200, userId, "Sale", 3, "Order", "1", null, null, null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithSaleOrderAndInsufficientStock_ReturnsValidationError()
    {
        // Arrange
        const long companyId = 100;
        const long userId = 5;
        const long warehouseId = 1;

        var dto = new CreateOrderDto(
            10,
            "Sale",
            null,
            null,
            warehouseId,
            new List<CreateOrderDetailDto> { new(100, 5, 10m), new(200, 10, 20m) });

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(companyId: companyId));
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(userId));

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInventory(warehouseId, 100, 50));
        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, 200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInventory(warehouseId, 200, 3));

        // Act
        var result = await _sut.CreateAsync(dto, companyId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient stock for SKU [200]", result.Message);
        Assert.Contains("Requested: 10", result.Message);
        Assert.Contains("Available: 3", result.Message);

        _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockKardexService.Verify(s => s.CreateEntryAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithSaleOrderAndNoStockRecord_ReturnsValidationError()
    {
        // Arrange
        const long companyId = 100;
        const long userId = 5;
        const long warehouseId = 1;

        var dto = new CreateOrderDto(
            10,
            "Sale",
            null,
            null,
            warehouseId,
            new List<CreateOrderDetailDto> { new(999, 5, 10m) });

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(companyId: companyId));
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(userId));

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SmartInventory?)null);

        // Act
        var result = await _sut.CreateAsync(dto, companyId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No stock record found for SKU [999]", result.Message);

        _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithReturnOrder_SkipsStockValidationAndCreatesKardexEntries()
    {
        // Arrange
        const long companyId = 100;
        const long userId = 5;
        const long warehouseId = 1;

        var dto = new CreateOrderDto(
            10,
            "Return",
            null,
            null,
            warehouseId,
            new List<CreateOrderDetailDto> { new(100, 5, 10m) });

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(companyId: companyId));
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(userId));

        var createdOrder = CreateOrder(1, companyId, 10, userId,
            warehouseId, "Return", totalAmount: 50m);

        _mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);
        _mockOrderRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _sut.CreateAsync(dto, companyId, userId);

        // Assert
        Assert.True(result.Success);

        _mockInventoryRepo.Verify(
            r => r.GetStockAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);

        _mockKardexService.Verify(s => s.CreateEntryAsync(
                companyId, warehouseId, 100, userId, "Return", 5, "Order", "1", null, null, null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithSaleOrderMissingWarehouseId_ReturnsValidationError()
    {
        // Arrange
        const long companyId = 100;
        const long userId = 5;

        var dto = new CreateOrderDto(
            10,
            "Sale",
            null,
            null,
            null,
            new List<CreateOrderDetailDto> { new(100, 5, 10m) });

        _mockCustomerRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(companyId: companyId));
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(userId));

        // Act
        var result = await _sut.CreateAsync(dto, companyId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("WarehouseId is required", result.Message);
    }

    #endregion
}
