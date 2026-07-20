namespace CSharpApp.Core.Dtos;

public sealed record Product
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("price")]
    public int? Price { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; } = [];

    [JsonPropertyName("creationAt")]
    public DateTime? CreationAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("category")]
    public Category? Category { get; set; }
}


public record CreateProductDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("price")] int Price,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("categoryId")] int CategoryId,
    [property: JsonPropertyName("images")] List<string> Images
);

public record UpdateProductDto(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("price")] int? Price,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("images")] List<string>? Images
);