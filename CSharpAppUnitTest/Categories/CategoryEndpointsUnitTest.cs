using System.Net;
using System.Net.Http.Json;
using CSharpApp.Application.Categories.Commands;
using CSharpApp.Application.Categories.Queries;
using CSharpApp.Core.Dtos;
using CSharpApp.Infrastructure.Helpers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CSharpApp.UnitTests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ISender> _senderMock;

    public CategoryEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _senderMock = new Mock<ISender>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _senderMock.Object);
            });
        });
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk_WhenResultIsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Books" }
        };

        // Wrap response in Result<T>.Success(...)
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<Category>>.Success(categories));

        // Act
        var response = await client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<Category>>();
        result.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFound_WhenResultIsFailed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var categoryId = 999;

        // Wrap response in Result<T>.Fail(...)
        _senderMock
            .Setup(s => s.Send(It.Is<GetCategoryByIdQuery>(q => q.Id == categoryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Category>.Failure("Category not found"));

        // Act
        var response = await client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenResultIsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateCategoryDto { Name = "Gaming" };
        var createdCategory = new Category { Id = 10, Name = "Gaming" };

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Category>.Success(createdCategory));

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<Category>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenResultIsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var categoryId = 1;

        _senderMock
            .Setup(s => s.Send(It.Is<DeleteCategoryCommand>(c => c.Id == categoryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var response = await client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
