using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace UnitTests;

public sealed class ProductsServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<ILogger<ProductsService>> _loggerMock = new();
    private readonly Mock<IOptionsSnapshot<RestApiSettings>> _options = new();


     private static List<Product> GetSampleProducts() =>
    [
        new()
        {
            Id = 1,
            Title = "Classic Black T-Shirt",
            Price = 25,
            Description = "A comfortable cotton t-shirt",
            Category = new Category { Id = 1, Name = "Clothes" }
        },
        new()
        {
            Id = 2,
            Title = "Leather Shoes",
            Price = 85,
            Description = "High quality leather shoes",
            Category = new Category { Id = 2, Name = "Shoes" }
        }
    ];

    private ProductsService CreateService()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.escuelajs.co/")
        };

        return new ProductsService(httpClient, _options.Object, _loggerMock.Object);
    }

    private void SetupMockResponse(HttpStatusCode statusCode, object? responseBody = null)
    {
        var response = new HttpResponseMessage(statusCode);

        if (responseBody is not null)
        {
            response.Content = JsonContent.Create(responseBody);
        }

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);
    }

    // --------------------------------------------------------------------------------
    // 1. GetProductsAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task GetProductsAsync_ReturnsProductsList_WhenSuccessful()
    {
        // Arrange
        var expectedProducts = GetSampleProducts();
        SetupMockResponse(HttpStatusCode.OK, expectedProducts);
        var service = CreateService();

        // Act
        var result = await service.GetProductsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Classic Black T-Shirt", result.First().Title);
        Assert.Equal("Clothes", result.First().Category?.Name);
    }

    // --------------------------------------------------------------------------------
    // 2. GetProductByIdAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task GetProductByIdAsync_ReturnsProduct_WhenFound()
    {
        // Arrange
        var expectedProduct = GetSampleProducts().First(); // Classic Black T-Shirt
        SetupMockResponse(HttpStatusCode.OK, expectedProduct);
        var service = CreateService();

        // Act
        var result = await service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Classic Black T-Shirt", result.Title);
        Assert.Equal(25, result.Price);
        Assert.Equal("Clothes", result.Category?.Name);
    }

    // --------------------------------------------------------------------------------
    // 3. CreateProductAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductAsync_ReturnsCreatedProduct_WhenSuccessful()
    {
        // Arrange
        var requestDto = new CreateProductDto(
            Title: "Classic Black T-Shirt",
            Price: 25,
            Description: "A comfortable cotton t-shirt",
            CategoryId: 1,
            Images: ["https://i.imgur.com/qR8y32.jpeg"]
        );

        var createdProduct = GetSampleProducts().First();

        SetupMockResponse(HttpStatusCode.Created, createdProduct);
        var service = CreateService();

        // Act
        var result = await service.CreateProductAsync(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Classic Black T-Shirt", result.Title);
        Assert.Equal("Clothes", result.Category?.Name);
    }

    // --------------------------------------------------------------------------------
    // 4. UpdateProductAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateProductAsync_ReturnsUpdatedProduct_WhenSuccessful()
    {
        // Arrange
        var updateDto = new UpdateProductDto(
            Title: "Leather Shoes Premium",
            Price: 95,
            Description: null,
            Images: null
        );

        var updatedProduct = GetSampleProducts().Last(); // Leather Shoes
        updatedProduct.Title = "Leather Shoes Premium";
        updatedProduct.Price = 95;

        SetupMockResponse(HttpStatusCode.OK, updatedProduct);
        var service = CreateService();

        // Act
        var result = await service.UpdateProductAsync(2, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Leather Shoes Premium", result.Title);
        Assert.Equal(95, result.Price);
        Assert.Equal("Shoes", result.Category?.Name);
    }

    // --------------------------------------------------------------------------------
    // 5. DeleteProductAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteProductAsync_ReturnsTrue_WhenDeletionSucceeds()
    {
        // Arrange
        SetupMockResponse(HttpStatusCode.OK, true);
        var service = CreateService();

        // Act
        var result = await service.DeleteProductAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteProductAsync_ReturnsFalse_WhenProductNotFound()
    {
        // Arrange
        SetupMockResponse(HttpStatusCode.NotFound);
        var service = CreateService();

        // Act
        var result = await service.DeleteProductAsync(999);

        // Assert
        Assert.False(result);
    }
}
