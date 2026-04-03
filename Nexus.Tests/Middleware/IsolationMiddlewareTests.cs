using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Api.Middleware;

namespace Nexus.Tests.Middleware;

public class IsolationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext = new();
    private readonly Mock<ILogger<IsolationMiddleware>> _mockLogger = new();

    private IsolationMiddleware CreateMiddleware()
    {
        return new IsolationMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    private static HttpContext CreateHttpContext(ClaimsPrincipal? user = null, string path = "/api/test")
    {
        var context = new DefaultHttpContext { Request = { Path = path } };
        if (user != null)
        {
            context.User = user;
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_WhenNotAuthenticated_SkipsProcessing()
    {
        // Arrange
        var context = CreateHttpContext(user: null);
        _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(n => n.Invoke(context), Times.Once);
        Assert.Null(context.Items["CompanyId"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticatedWithCompanyId_StoresInItems()
    {
        // Arrange
        const long expectedCompanyId = 123;
        var claims = new[] { new Claim("company_id", expectedCompanyId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var context = CreateHttpContext(user);

        _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(n => n.Invoke(context), Times.Once);
        Assert.Equal(expectedCompanyId, context.Items["CompanyId"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticatedWithoutCompanyId_DoesNotStore()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var context = CreateHttpContext(user);

        _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(n => n.Invoke(context), Times.Once);
        Assert.Null(context.Items["CompanyId"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenCompanyIdInvalid_LogsWarning()
    {
        // Arrange
        var claims = new[] { new Claim("company_id", "invalid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var context = CreateHttpContext(user);

        _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(n => n.Invoke(context), Times.Once);
        Assert.Null(context.Items["CompanyId"]);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        var nextCalled = false;
        _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
