using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Nexus.Application.UseCases.Auth;

namespace Nexus.Tests.Auth;

public class ClaimsExtractorTests
{
    private readonly Mock<IHttpContextAccessor> _mockContextAccessor;
    private readonly ClaimsExtractor _sut;

    public ClaimsExtractorTests()
    {
        _mockContextAccessor = new Mock<IHttpContextAccessor>();
        _sut = new ClaimsExtractor(_mockContextAccessor.Object);
    }

    private void SetupHttpContext(ClaimsPrincipal? user)
    {
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.User).Returns(user ?? new ClaimsPrincipal());

        // Use a mock that returns null for any key (simulates missing key)
        var mockItems = new Mock<IDictionary<object, object?>>();
        mockItems.Setup(m => m[It.IsAny<object>()]).Returns((object? _) => null);
        mockContext.SetupGet(c => c.Items).Returns(mockItems.Object);

        _mockContextAccessor.Setup(c => c.HttpContext).Returns(mockContext.Object);
    }

    #region GetCurrentCompanyId Tests

    [Fact]
    public void GetCurrentCompanyId_WhenSetInItems_ReturnsCompanyIdFromItems()
    {
        // Arrange
        const long expectedCompanyId = 123;
        var mockContext = new Mock<HttpContext>();
        var itemsDict = new Dictionary<object, object?> { ["CompanyId"] = expectedCompanyId };
        mockContext.SetupGet(c => c.Items).Returns(itemsDict);
        mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal());
        _mockContextAccessor.Setup(c => c.HttpContext).Returns(mockContext.Object);

        // Act
        var result = _sut.GetCurrentCompanyId();

        // Assert
        Assert.Equal(expectedCompanyId, result);
    }

    [Fact]
    public void GetCurrentCompanyId_WhenNotInItems_FallsBackToClaims()
    {
        // Arrange
        const long expectedCompanyId = 456;
        var claims = new[]
        {
            new Claim("company_id", expectedCompanyId.ToString()), new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.User).Returns(user);

        // Mock Items to return null for missing key
        var mockItems = new Mock<IDictionary<object, object?>>();
        mockItems.Setup(m => m[It.IsAny<object>()]).Returns((object? _) => null);
        mockContext.SetupGet(c => c.Items).Returns(mockItems.Object);

        _mockContextAccessor.Setup(c => c.HttpContext).Returns(mockContext.Object);

        // Act
        var result = _sut.GetCurrentCompanyId();

        // Assert
        Assert.Equal(expectedCompanyId, result);
    }

    [Fact]
    public void GetCurrentCompanyId_WhenNoHttpContext_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockContextAccessor.Setup(c => c.HttpContext).Returns((HttpContext?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentCompanyId());
    }

    [Fact]
    public void GetCurrentCompanyId_WhenClaimMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.User).Returns(user);

        var mockItems = new Mock<IDictionary<object, object?>>();
        mockItems.Setup(m => m[It.IsAny<object>()]).Returns((object? _) => null);
        mockContext.SetupGet(c => c.Items).Returns(mockItems.Object);

        _mockContextAccessor.Setup(c => c.HttpContext).Returns(mockContext.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentCompanyId());
    }

    [Fact]
    public void GetCurrentCompanyId_WhenClaimInvalid_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new Claim("company_id", "invalid"), new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.User).Returns(user);

        var mockItems = new Mock<IDictionary<object, object?>>();
        mockItems.Setup(m => m[It.IsAny<object>()]).Returns((object? _) => null);
        mockContext.SetupGet(c => c.Items).Returns(mockItems.Object);

        _mockContextAccessor.Setup(c => c.HttpContext).Returns(mockContext.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentCompanyId());
    }

    #endregion

    #region GetCurrentUserId Tests

    [Fact]
    public void GetCurrentUserId_WhenClaimExists_ReturnsUserId()
    {
        // Arrange
        const long expectedUserId = 789;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        SetupHttpContext(user);

        // Act
        var result = _sut.GetCurrentUserId();

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public void GetCurrentUserId_WhenClaimMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        SetupHttpContext(user);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentUserId());
    }

    [Fact]
    public void GetCurrentUserId_WhenClaimInvalid_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "invalid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        SetupHttpContext(user);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentUserId());
    }

    #endregion

    #region GetCurrentRoles Tests

    [Fact]
    public void GetCurrentRoles_WhenRolesExist_ReturnsRoleList()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.Role, "User") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        SetupHttpContext(user);

        // Act
        var result = _sut.GetCurrentRoles();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Admin", result);
        Assert.Contains("User", result);
    }

    [Fact]
    public void GetCurrentRoles_WhenNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        SetupHttpContext(user);

        // Act
        var result = _sut.GetCurrentRoles();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetCurrentPermissions Tests

    [Fact]
    public void GetCurrentPermissions_WhenPermissionsExist_ReturnsPermissionList()
    {
        // Arrange
        var claims = new[] { new Claim("permission", "read:users"), new Claim("permission", "write:users") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        SetupHttpContext(user);

        // Act
        var result = _sut.GetCurrentPermissions();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("read:users", result);
        Assert.Contains("write:users", result);
    }

    [Fact]
    public void GetCurrentPermissions_WhenNoPermissions_ReturnsEmptyList()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        SetupHttpContext(user);

        // Act
        var result = _sut.GetCurrentPermissions();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region TryGetCurrentCompanyId Tests

    [Fact]
    public void TryGetCurrentCompanyId_WhenSuccessful_ReturnsTrue()
    {
        // Arrange
        const long expectedCompanyId = 999;
        var mockContext = new Mock<HttpContext>();
        mockContext.SetupGet(c => c.Items)
            .Returns(new Dictionary<object, object?> { ["CompanyId"] = expectedCompanyId });
        mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal());
        _mockContextAccessor.Setup(c => c.HttpContext).Returns(mockContext.Object);

        // Act
        var success = _sut.TryGetCurrentCompanyId(out var companyId);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedCompanyId, companyId);
    }

    [Fact]
    public void TryGetCurrentCompanyId_WhenFails_ReturnsFalse()
    {
        // Arrange
        _mockContextAccessor.Setup(c => c.HttpContext).Returns((HttpContext?)null);

        // Act
        var success = _sut.TryGetCurrentCompanyId(out var companyId);

        // Assert
        Assert.False(success);
        Assert.Equal(0, companyId);
    }

    #endregion
}
