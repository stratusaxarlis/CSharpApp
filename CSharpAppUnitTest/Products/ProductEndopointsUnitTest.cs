using System.Net;
using System.Net.Http.Json;
using CSharpApp.Application.Products.Commands;
using CSharpApp.Application.Products.Queries;
using CSharpApp.Core.Dtos;
using CSharpApp.Infrastructure.Helpers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CSharpApp.UnitTests.Endpoints;

public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ISender> _senderMock;

    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
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

    // -------------------------------------------------------------------------
    // GET /api/products
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProducts_ShouldReturnOk_WhenProductsExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var products = new List<Product>
        {
            new() { Id = 1, Title = "Laptop", Price = 1200, Category = new Category { Id = 1, Name = "Electronics" } },
            new() { Id = 2, Title = "Wireless Mouse", Price = 25, Category = new Category { Id = 1, Name = "Electronics" } }
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<Product>>.Success(products));

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Product>>();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(products);
    }

    // -------------------------------------------------------------------------
    // GET /api/products/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProductById_ShouldReturnOk_WhenProductExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = 1;
        var product = new Product { Id = productId, Title = "Laptop", Price = 1200, Category = new Category { Id = 1, Name = "Electronics" } };

        _senderMock
            .Setup(s => s.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Product>.Success(product));

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Product>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Title.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = 999;

        _senderMock
            .Setup(s => s.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Product>.Failure("Product not found"));

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -------------------------------------------------------------------------
    // POST /api/products
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenPayloadIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateProductDto ( Title : "Mechanical Keyboard", Price : 99,  Description : "Mechanical Keyboard",CategoryId : 1,[] );
        var createdProduct = new Product { Id = 10, Title = "Mechanical Keyboard", Price = 99, Category = new Category { Id = 1, Name = "Electronics" } };

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Product>.Success(createdProduct));

        // Act
        var response = await client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<Product>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Title.Should().Be("Mechanical Keyboard");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidDto = new CreateProductDto (Title : "", Price : -10, Description : "", CategoryId : 0 ,[]);

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Product>.Failure("Invalid product data provided"));

        // Act
        var response = await client.PostAsJsonAsync("/api/products", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // -------------------------------------------------------------------------
    // PUT /api/products/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateProduct_ShouldReturnNoContent_WhenUpdateSucceeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = 1;
        var updateDto = new UpdateProductDto( Title : "Updated Laptop", Price : 110, Description : "Updated Laptop", []);

        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Product>.Success(new Product { Id = productId, Title = "Updated Laptop", Price = 110, Category = new Category { Id = 1, Name = "Electronics" } }));

        // Act
        var response = await client.PutAsJsonAsync($"/api/products/{productId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = 999;
        var updateDto = new UpdateProductDto( Title : "Nonexistent", Price : 50, Description : "Nonexistent", []);

        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Product>.Failure("Product not found"));

        // Act
        var response = await client.PutAsJsonAsync($"/api/products/{productId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/products/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent_WhenDeletionSucceeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = 1;

        _senderMock
            .Setup(s => s.Send(It.Is<DeleteProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var response = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = 999;

        _senderMock
            .Setup(s => s.Send(It.Is<DeleteProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("Product not found"));

        // Act
        var response = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
