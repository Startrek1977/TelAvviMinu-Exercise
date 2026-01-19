using System.Text.Json;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Infrastructure;

public class JsonSerializerTests
{
    [Fact]
    public void FileExtension_ReturnsJson()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();

        // Act & Assert
        Assert.Equal(".json", serializer.FileExtension);
    }

    [Fact]
    public async Task SerializeAsync_ReturnsValidJson()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };

        // Act
        var json = await serializer.SerializeAsync(products);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Product 1", json);
        Assert.Contains("P001", json);
    }

    [Fact]
    public async Task SerializeAsync_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => serializer.SerializeAsync(null!));
    }

    [Fact]
    public async Task DeserializeAsync_ReturnsProducts_WhenValidJson()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();
        var json = "[{\"Id\":1,\"Name\":\"Test\",\"Code\":\"T001\",\"Category\":\"Cat\",\"Price\":10.00,\"Stock\":100}]";

        // Act
        var result = await serializer.DeserializeAsync(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public async Task DeserializeAsync_ReturnsEmptyCollection_WhenJsonIsNull()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();

        // Act
        var result = await serializer.DeserializeAsync(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeserializeAsync_ReturnsEmptyCollection_WhenJsonIsEmpty()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();

        // Act
        var result = await serializer.DeserializeAsync("");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeserializeAsync_ReturnsEmptyCollection_WhenJsonIsWhitespace()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();

        // Act
        var result = await serializer.DeserializeAsync("   ");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeserializeAsync_ReturnsEmptyCollection_WhenJsonIsInvalid()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>();

        // Act
        var result = await serializer.DeserializeAsync("{ invalid json }");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Constructor_AcceptsNullOptions_UsesSystemDefaults()
    {
        // Arrange
        var serializer = new JsonSerializer<Product>(null);
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };

        // Act
        var json = await serializer.SerializeAsync(products);

        // Assert - System.Text.Json defaults: PascalCase, not indented
        Assert.Contains("\"Id\":", json);
        Assert.Contains("\"Name\":", json);
    }

    [Fact]
    public async Task Constructor_UsesCustomOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        var serializer = new JsonSerializer<Product>(options);
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };

        // Act
        var json = await serializer.SerializeAsync(products);

        // Assert - camelCase property names
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"name\":", json);
        Assert.DoesNotContain("\n", json); // Not indented
    }
}
