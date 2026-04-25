using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.UseCases.Products;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Security;

namespace Nexus.Tests.Products.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<ICompanyRepository> _mockCompanyRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<IValidator<CreateProductDto>> _mockCreateValidator;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
        _mockCompanyRepo = new Mock<ICompanyRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockCreateValidator = new Mock<IValidator<CreateProductDto>>();
        var mockUpdateValidator = new Mock<IValidator<UpdateProductDto>>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        // Default validation success
        _mockCreateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateProductDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        mockUpdateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateProductDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new ProductService(
            _mockProductRepo.Object,
            _mockCompanyRepo.Object,
            _mockCategoryRepo.Object,
            _mockCreateValidator.Object,
            mockUpdateValidator.Object,
            mockLogger.Object
        );
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;
        var product = CreateTestProduct(productId, companyId, "Test Product");

        _mockProductRepo
            .Setup(r => r.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(productId, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(productId, result.Data.Id);
        Assert.Equal("Test Product", result.Data.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;

        _mockProductRepo
            .Setup(r => r.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetByIdAsync(productId, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCompanyMismatch_ReturnsNotFound()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;
        var product = CreateTestProduct(productId, 999L, "Test Product"); // Different company

        _mockProductRepo
            .Setup(r => r.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(productId, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    #endregion

    #region GetByCompanyAsync

    [Fact]
    public async Task GetByCompanyAsync_ReturnsProductsForCompany()
    {
        // Arrange
        const long companyId = 10L;
        var products = new List<Product>
        {
            CreateTestProduct(1, companyId, "Product 1"), CreateTestProduct(2, companyId, "Product 2")
        };

        _mockProductRepo
            .Setup(r => r.GetByCompanyWithCategoryAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetByCompanyAsync(companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        const long companyId = 10L;
        var dto = new CreateProductDto(1L, "New Product", "Brand");
        var company = new Company { Id = companyId, Name = "Test Company", TaxId = "12345678" };

        _mockCompanyRepo
            .Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _mockProductRepo
            .Setup(r => r.ExistsByNameAsync(companyId, dto.Name, default))
            .ReturnsAsync(false);

        _mockProductRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) =>
            {
                p.Id = 1;
                return p;
            });

        // Act
        var result = await _sut.CreateAsync(dto, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("New Product", result.Data.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenCompanyNotFound_ReturnsNotFound()
    {
        // Arrange
        const long companyId = 10L;
        var dto = new CreateProductDto(null, "New Product", "Brand");

        _mockCompanyRepo
            .Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _sut.CreateAsync(dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_ReturnsValidationError()
    {
        // Arrange
        const long companyId = 10L;
        var dto = new CreateProductDto(null, "New Product", "Brand");

        var validationResult = new ValidationResult(new List<ValidationFailure> { new("Name", "Name is required") });

        _mockCreateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateProductDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _sut.CreateAsync(dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.ValidationError, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        const long companyId = 10L;
        const long categoryId = 1L;
        var dto = new CreateProductDto(categoryId, "New Product", "Brand");
        var company = new Company { Id = companyId, Name = "Test Company", TaxId = "12345678" };

        _mockCompanyRepo
            .Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _mockCategoryRepo
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _sut.CreateAsync(dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateName_ReturnsConflict()
    {
        // Arrange
        const long companyId = 10L;
        var dto = new CreateProductDto(null, "Existing Product", "Brand");
        var company = new Company { Id = companyId, Name = "Test Company", TaxId = "12345678" };

        _mockCompanyRepo
            .Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _mockProductRepo
            .Setup(r => r.ExistsByNameAsync(companyId, dto.Name, default))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.Conflict, result.ErrorCode);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;
        var existingProduct = CreateTestProduct(productId, companyId, "Old Name");
        var dto = new UpdateProductDto(null, "New Name", "New Brand");

        _mockProductRepo
            .Setup(r => r.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(r => r.ExistsByNameAsync(companyId, dto.Name, productId, default))
            .ReturnsAsync(false);

        _mockProductRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(productId, dto, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("New Name", result.Data!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;
        var dto = new UpdateProductDto(null, "New Name", "New Brand");

        _mockProductRepo
            .Setup(r => r.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.UpdateAsync(productId, dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateName_ReturnsConflict()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;
        var existingProduct = CreateTestProduct(productId, companyId, "Existing");
        var dto = new UpdateProductDto(null, "Duplicate Name", "Brand");

        _mockProductRepo
            .Setup(r => r.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockProductRepo
            .Setup(r => r.ExistsByNameAsync(companyId, dto.Name, productId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(productId, dto, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.Conflict, result.ErrorCode);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ReturnsTrue()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;
        var product = CreateTestProduct(productId, companyId, "Test Product");

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockProductRepo
            .Setup(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(productId, companyId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        const long productId = 1L;
        const long companyId = 10L;

        _mockProductRepo
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.DeleteAsync(productId, companyId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    #endregion

    #region Helpers

    private static Product CreateTestProduct(long id, long companyId, string name)
    {
        return new Product
        {
            Id = id,
            CompanyId = companyId,
            Name = name,
            Brand = "Test Brand",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
