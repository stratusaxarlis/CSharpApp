using System.Net;
using System.Net.Http.Json;
using CSharpApp.Application.Categories;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CSharpAppUnitTest.Categories;
public sealed class CategoriesServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<ILogger<CategoriesService>> _loggerMock = new();
    private readonly Mock<IOptionsSnapshot<RestApiSettings>> _options = new();

    private static List<Category> GetSampleCategories() =>
    [
        new()
        {
            Id = 1,
            Name = "Updated Category Name",
            Image = "https://placeimg.com/640/480/any",
            CreationAt = DateTime.Parse("2026-07-20T06:52:36.000Z"),
            UpdatedAt = DateTime.Parse("2026-07-20T13:53:06.000Z")
        },
        new()
        {
            Id = 2,
            Name = "Electronics",
            Image = "https://i.imgur.com/ZANVnHE.jpeg",
            CreationAt = DateTime.Parse("2026-07-20T06:52:36.000Z"),
            UpdatedAt = DateTime.Parse("2026-07-20T06:52:36.000Z")
        }
    ];

    private CategoriesService CreateService()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.escuelajs.co/")
        };

        return new CategoriesService(httpClient, _options.Object, _loggerMock.Object);
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
    // 1. GetCategoriesAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategoriesList_WhenSuccessful()
    {
        // Arrange
        var expectedCategories = GetSampleCategories();
        SetupMockResponse(HttpStatusCode.OK, expectedCategories);
        var service = CreateService();

        // Act
        var result = await service.GetCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Updated Category Name", result.First().Name);
        Assert.Equal("Electronics", result.Last().Name);
    }

    // --------------------------------------------------------------------------------
    // 2. GetCategoryByIdAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsCategory_WhenFound()
    {
        // Arrange
        var expectedCategory = GetSampleCategories().First(); // Updated Category Name
        SetupMockResponse(HttpStatusCode.OK, expectedCategory);
        var service = CreateService();

        // Act
        var result = await service.GetCategoryByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Updated Category Name", result.Name);
        Assert.Equal("https://placeimg.com/640/480/any", result.Image);
    }

    // --------------------------------------------------------------------------------
    // 3. CreateCategoryAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task CreateCategoryAsync_ReturnsCreatedCategory_WhenSuccessful()
    {
        // Arrange
        var requestDto = new CreateCategoryDto();
        requestDto.Name = "Created Category Name";
        requestDto.Image = "https://placeimg.com/640/480/any";


        var createdCategory = GetSampleCategories().Last(); // Electronics

        SetupMockResponse(HttpStatusCode.Created, createdCategory);
        var service = CreateService();

        // Act
        var result = await service.CreateCategoryAsync(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal("https://i.imgur.com/ZANVnHE.jpeg", result.Image);
    }

    // --------------------------------------------------------------------------------
    // 4. UpdateCategoryAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateCategoryAsync_ReturnsUpdatedCategory_WhenSuccessful()
    {
        // Arrange
        UpdateCategoryDto updateDto = new UpdateCategoryDto();
        updateDto.Name = "Updated Category Name";
        updateDto.Image = "https://placeimg.com/640/480/any";

        var updatedCategory = GetSampleCategories().First();

        SetupMockResponse(HttpStatusCode.OK, updatedCategory);
        var service = CreateService();

        // Act
        var result = await service.UpdateCategoryAsync(1, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Updated Category Name", result.Name);
        Assert.Equal("https://placeimg.com/640/480/any", result.Image);
    }

    // --------------------------------------------------------------------------------
    // 5. DeleteCategoryAsync Tests
    // --------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteCategoryAsync_ReturnsTrue_WhenDeletionSucceeds()
    {
        // Arrange
        SetupMockResponse(HttpStatusCode.OK, true);
        var service = CreateService();

        // Act
        var result = await service.DeleteCategoryAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ReturnsFalse_WhenCategoryNotFound()
    {
        // Arrange
        SetupMockResponse(HttpStatusCode.NotFound);
        var service = CreateService();

        // Act
        var result = await service.DeleteCategoryAsync(999);

        // Assert
        Assert.False(result);
    }
}
