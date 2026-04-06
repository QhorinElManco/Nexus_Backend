using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Dto.Products;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.UseCases.Products;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Tests.Products;

public class KardexEntryServiceTests
{
    private readonly Mock<IKardexEntryRepository> _mockKardexRepo;
    private readonly Mock<ISmartInventoryRepository> _mockInventoryRepo;
    private readonly Mock<ILogger<KardexEntryService>> _mockLogger;
    private readonly KardexEntryService _sut;

    public KardexEntryServiceTests()
    {
        _mockKardexRepo = new Mock<IKardexEntryRepository>();
        _mockInventoryRepo = new Mock<ISmartInventoryRepository>();
        _mockLogger = new Mock<ILogger<KardexEntryService>>();
        _sut = new KardexEntryService(_mockKardexRepo.Object, _mockInventoryRepo.Object, _mockLogger.Object);
    }

    private static KardexEntry CreateKardexEntry(long id = 1, long companyId = 100, long warehouseId = 1,
        long skuId = 10, long userId = 5, string transactionType = "Sale", int quantity = 5,
        int stockBefore = 100, int stockAfter = 95)
    {
        return new KardexEntry
        {
            Id = id,
            CompanyId = companyId,
            WarehouseId = warehouseId,
            SkuId = skuId,
            UserId = userId,
            TransactionType = transactionType,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            ReferenceDocType = "Order",
            ReferenceDocId = "1"
        };
    }

    private static SmartInventory CreateSmartInventory(long id = 1, long warehouseId = 1, long skuId = 10,
        int currentQuantity = 100)
    {
        return new SmartInventory
        {
            Id = id,
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

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenEntryExistsAndCompanyMatches_ReturnsEntry()
    {
        // Arrange
        const long entryId = 1;
        const long companyId = 100;
        var entry = CreateKardexEntry(entryId, companyId);

        _mockKardexRepo.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.GetByIdAsync(entryId, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(entryId, result.Data.Id);
        Assert.Equal("Sale", result.Data.TransactionType);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntryNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockKardexRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((KardexEntry?)null);

        // Act
        var result = await _sut.GetByIdAsync(999, 100);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Kardex entry not found", result.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCompanyMismatch_ReturnsNotFound()
    {
        // Arrange
        const long entryId = 1;
        var entry = CreateKardexEntry(entryId, 100);

        _mockKardexRepo.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        // Act
        var result = await _sut.GetByIdAsync(entryId, 200);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Kardex entry not found", result.Message);
    }

    #endregion

    #region GetByWarehouseAsync Tests

    [Fact]
    public async Task GetByWarehouseAsync_FiltersByCompany()
    {
        // Arrange
        const long warehouseId = 1;
        const long companyId = 100;
        var entries = new List<KardexEntry>
        {
            CreateKardexEntry(1, 100, 1, 10),
            CreateKardexEntry(2, 200, 1, 11),
            CreateKardexEntry(3, 100, 1, 12, transactionType: "Return", stockBefore: 47, stockAfter: 49)
        };

        _mockKardexRepo.Setup(r => r.GetByWarehouseAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        // Act
        var result = await _sut.GetByWarehouseAsync(warehouseId, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.All(result.Data, e => Assert.Equal(companyId, e.CompanyId));
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_ReturnsPaginatedResults()
    {
        // Arrange
        const long companyId = 100;
        var request = new KardexEntrySearchRequest(1, 10);
        var items = new List<KardexEntry> { CreateKardexEntry(companyId: companyId) };

        _mockKardexRepo.Setup(r => r.SearchAsync(request, companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 1));

        // Act
        var result = await _sut.SearchAsync(request, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal(1, result.Pagination.Page);
        Assert.Equal(10, result.Pagination.PageSize);
        Assert.Equal(1, result.Pagination.TotalRecords);
    }

    #endregion

    #region CreateEntryAsync Tests

    [Fact]
    public async Task CreateEntryAsync_WithSaleTransactionType_DeductsStock()
    {
        // Arrange
        const long companyId = 100;
        const long warehouseId = 1;
        const long skuId = 10;
        const long userId = 5;
        const int quantity = 10;
        const int currentQuantity = 100;

        var inventory = CreateSmartInventory(warehouseId: warehouseId, skuId: skuId, currentQuantity: currentQuantity);

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        _mockInventoryRepo.Setup(r => r.UpdateStockAsync(warehouseId, skuId, -quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateEntryAsync(companyId, warehouseId, skuId, userId,
            "Sale", quantity, "Order", "1", ct: CancellationToken.None);

        // Assert
        _mockInventoryRepo.Verify(r => r.UpdateStockAsync(warehouseId, skuId, -quantity, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockKardexRepo.Verify(r => r.AddAsync(It.Is<KardexEntry>(e =>
            e.TransactionType == "Sale" &&
            e.Quantity == quantity &&
            e.StockBefore == currentQuantity &&
            e.StockAfter == currentQuantity - quantity &&
            e.ReferenceDocType == "Order" &&
            e.ReferenceDocId == "1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEntryAsync_WithReturnTransactionType_AddsStock()
    {
        // Arrange
        const long companyId = 100;
        const long warehouseId = 1;
        const long skuId = 10;
        const long userId = 5;
        const int quantity = 5;
        const int currentQuantity = 50;

        var inventory = CreateSmartInventory(warehouseId: warehouseId, skuId: skuId, currentQuantity: currentQuantity);

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        _mockInventoryRepo.Setup(r => r.UpdateStockAsync(warehouseId, skuId, quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateEntryAsync(companyId, warehouseId, skuId, userId,
            "Return", quantity, "Order", "2", ct: CancellationToken.None);

        // Assert
        _mockInventoryRepo.Verify(r => r.UpdateStockAsync(warehouseId, skuId, quantity, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockKardexRepo.Verify(r => r.AddAsync(It.Is<KardexEntry>(e =>
            e.TransactionType == "Return" &&
            e.Quantity == quantity &&
            e.StockBefore == currentQuantity &&
            e.StockAfter == currentQuantity + quantity), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEntryAsync_WithPurchaseTransactionType_AddsStock()
    {
        // Arrange
        const long companyId = 100;
        const long warehouseId = 1;
        const long skuId = 10;
        const long userId = 5;
        const int quantity = 20;
        const int currentQuantity = 30;

        var inventory = CreateSmartInventory(warehouseId: warehouseId, skuId: skuId, currentQuantity: currentQuantity);

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        _mockInventoryRepo.Setup(r => r.UpdateStockAsync(warehouseId, skuId, quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateEntryAsync(companyId, warehouseId, skuId, userId,
            "Purchase", quantity, "Delivery", "3", ct: CancellationToken.None);

        // Assert
        _mockInventoryRepo.Verify(r => r.UpdateStockAsync(warehouseId, skuId, quantity, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockKardexRepo.Verify(r => r.AddAsync(It.Is<KardexEntry>(e =>
            e.TransactionType == "Purchase" &&
            e.StockAfter == currentQuantity + quantity), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEntryAsync_WithInvalidTransactionType_ThrowsArgumentException()
    {
        // Arrange
        const long companyId = 100;
        const long warehouseId = 1;
        const long skuId = 10;
        const long userId = 5;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateEntryAsync(companyId, warehouseId, skuId, userId,
                "Invalid", 10, "Order", "1", ct: CancellationToken.None));

        Assert.Contains("Invalid TransactionType", exception.Message);
    }

    [Fact]
    public async Task CreateEntryAsync_WhenNoStockRecord_ThrowsInvalidOperationException()
    {
        // Arrange
        const long companyId = 100;
        const long warehouseId = 1;
        const long skuId = 999;
        const long userId = 5;

        _mockInventoryRepo.Setup(r => r.GetStockAsync(warehouseId, skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SmartInventory?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateEntryAsync(companyId, warehouseId, skuId, userId,
                "Sale", 10, "Order", "1", ct: CancellationToken.None));

        Assert.Contains("No stock record found", exception.Message);
    }

    #endregion

    #region ReconcileAsync Tests

    [Fact]
    public async Task ReconcileAsync_WhenNoDiscrepancies_ReturnsSuccessWithZeroDiscrepancies()
    {
        // Arrange
        const long companyId = 100;
        var inventories = new List<SmartInventory>
        {
            CreateSmartInventory(warehouseId: 1, skuId: 10, currentQuantity: 90)
        };

        var kardexEntries = new List<KardexEntry>
        {
            CreateKardexEntry(companyId: companyId, warehouseId: 1, skuId: 10, quantity: 10, stockBefore: 100,
                stockAfter: 90)
        };

        _mockInventoryRepo.Setup(r => r.GetAllByCompanyAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventories);

        _mockKardexRepo.Setup(r => r.GetByCompanyAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(kardexEntries);

        // Act
        var result = await _sut.ReconcileAsync(companyId, false);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.HasDiscrepancies);
        Assert.Empty(result.Data.Discrepancies);
    }

    [Fact]
    public async Task ReconcileAsync_WhenDiscrepancyDetected_ReturnsDiscrepancyList()
    {
        // Arrange
        const long companyId = 100;
        var inventories = new List<SmartInventory>
        {
            // Stored says 100, but history sums to 90
            CreateSmartInventory(warehouseId: 1, skuId: 10, currentQuantity: 100)
        };

        var kardexEntries = new List<KardexEntry>
        {
            CreateKardexEntry(companyId: companyId, warehouseId: 1, skuId: 10, quantity: 10, stockBefore: 100,
                stockAfter: 90)
        };

        _mockInventoryRepo.Setup(r => r.GetAllByCompanyAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventories);

        _mockKardexRepo.Setup(r => r.GetByCompanyAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(kardexEntries);

        // Act
        var result = await _sut.ReconcileAsync(companyId, false);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasDiscrepancies);
        Assert.Single(result.Data.Discrepancies);
        var discrepancy = result.Data.Discrepancies[0];
        Assert.Equal(100, discrepancy.StoredQuantity);
        Assert.Equal(90, discrepancy.CalculatedQuantity);
        Assert.Equal(-10, discrepancy.Difference);
        Assert.False(discrepancy.Corrected);
    }

    [Fact]
    public async Task ReconcileAsync_WithCorrection_UpdatesStockAndCreatesAdjustmentEntry()
    {
        // Arrange
        const long companyId = 100;
        var inventories = new List<SmartInventory> { CreateSmartInventory(1, 1, 10, 100) };

        var kardexEntries = new List<KardexEntry>
        {
            CreateKardexEntry(companyId: companyId, warehouseId: 1, skuId: 10, quantity: 10, stockBefore: 100,
                stockAfter: 90)
        };

        _mockInventoryRepo.Setup(r => r.GetAllByCompanyAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventories);

        _mockKardexRepo.Setup(r => r.GetByCompanyAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(kardexEntries);

        _mockInventoryRepo.Setup(r => r.UpdateStockAsync(1, 10, -10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.ReconcileAsync(companyId, true);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasDiscrepancies);
        Assert.Single(result.Data.Discrepancies);
        Assert.True(result.Data.Discrepancies[0].Corrected);
        Assert.Equal(1, result.Data.CorrectedCount);

        // Verify stock update was called
        _mockInventoryRepo.Verify(r => r.UpdateStockAsync(1, 10, -10, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Adjustment KardexEntry was created
        _mockKardexRepo.Verify(r => r.AddAsync(It.Is<KardexEntry>(e =>
            e.TransactionType == "Adjustment" &&
            e.ReferenceDocType == "Reconciliation" &&
            e.StockBefore == 100 &&
            e.StockAfter == 90 &&
            e.DeviceId == "system-reconciliation"), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
