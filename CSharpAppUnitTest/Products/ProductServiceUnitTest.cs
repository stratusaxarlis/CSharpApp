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

public class ProductsServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly Mock<ILogger<ProductsService>> _loggerMock = new();
    private readonly IOptions<RestApiSettings> _apiSettings;

    public ProductsServiceTests()
    {
        _apiSettings = Options.Create(new RestApiSettings
        {
            BaseUrl = "https://api.escuelajs.co/",
            Products = "api/v1/products"
        });
    }

    [Fact]
    public async Task GetProducts_ReturnsProductList_WhenApiReturns200OK()
    {
        // Arrange
        var mockProducts = new List<Product>
        {
            new ()
            {
                Id = 1,
                Title = "Classic Black T-Shirt",
                Price = 25,
                Description = "A comfortable cotton t-shirt",
                Category = new Category { Id = 1, Name = "Clothes" }
            },
            new ()
            {
                Id = 2,
                Title = "Leather Shoes",
                Price = 85,
                Description = "High quality leather shoes",
                Category = new Category { Id = 2, Name = "Shoes" }
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(mockProducts)
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri(_apiSettings.Value.BaseUrl!)
        };

        ProductsService service = new (httpClient, _apiSettings, _loggerMock.Object);

        // Act
        IReadOnlyCollection<Product> result = await service.GetProducts();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstProduct = result.First();
        Assert.Equal(1, firstProduct.Id);
        Assert.Equal("Classic Black T-Shirt", firstProduct.Title);
        Assert.Equal(25, firstProduct.Price);
        Assert.Equal("Clothes", firstProduct.Category?.Name);
    }

    [Fact]
    public async Task GetProducts_ThrowsHttpRequestException_WhenApiReturnsServerError()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri(_apiSettings.Value.BaseUrl!)
        };

        var service = new ProductsService(httpClient, _apiSettings, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetProducts());
    }
}