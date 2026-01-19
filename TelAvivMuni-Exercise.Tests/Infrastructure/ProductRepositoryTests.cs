using System.IO;
using System.Text.Json;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Infrastructure;

public class ProductRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFilePath;

    public ProductRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ProductRepoTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _testFilePath = Path.Combine(_testDirectory, "Products.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private ProductRepository CreateRepository(string? filePath = null)
    {
        var path = filePath ?? _testFilePath;
        var dataStore = new FileDataStore<Product>(path, new JsonSerializer<Product>());
        return new ProductRepository(dataStore);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyArray_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent.json");
        var repository = CreateRepository(nonExistentPath);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProducts_WhenFileExists()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Category A", Price = 10.00m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Code = "P002", Category = "Category B", Price = 20.00m, Stock = 200 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("Product 1", result[0].Name);
        Assert.Equal("Product 2", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Category A", Price = 10.00m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Code = "P002", Category = "Category B", Price = 20.00m, Stock = 200 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Product 2", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Category A", Price = 10.00m, Stock = 100 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AssignsId_WhenIdIsZero()
    {
        // Arrange
        var existingProducts = new List<Product>
        {
            new() { Id = 1, Name = "Existing", Code = "E001", Category = "Cat", Price = 5.00m, Stock = 50 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(existingProducts));
        var repository = CreateRepository();

        var newProduct = new Product { Id = 0, Name = "New Product", Code = "N001", Category = "Cat", Price = 15.00m, Stock = 10 };

        // Act
        await repository.AddAsync(newProduct);

        // Assert
        Assert.Equal(2, newProduct.Id);
        var allProducts = await repository.GetAllAsync();
        Assert.Equal(2, allProducts.Length);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenEntityIsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.AddAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cannot be null", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenDuplicateId()
    {
        // Arrange
        var existingProducts = new List<Product>
        {
            new() { Id = 1, Name = "Existing", Code = "E001", Category = "Cat", Price = 5.00m, Stock = 50 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(existingProducts));
        var repository = CreateRepository();

        var duplicateProduct = new Product { Id = 1, Name = "Duplicate", Code = "D001", Category = "Cat", Price = 10.00m, Stock = 100 };

        // Act
        var result = await repository.AddAsync(duplicateProduct);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingProduct()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Original", Code = "O001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        var updatedProduct = new Product { Id = 1, Name = "Updated", Code = "U001", Category = "NewCat", Price = 25.00m, Stock = 50 };

        // Act
        await repository.UpdateAsync(updatedProduct);

        // Assert
        var result = await repository.GetByIdAsync(1);
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("U001", result.Code);
        Assert.Equal(25.00m, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenEntityIsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.UpdateAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cannot be null", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenEntityNotFound()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        var nonExistentProduct = new Product { Id = 999, Name = "NonExistent", Code = "N001", Category = "Cat", Price = 10.00m, Stock = 100 };

        // Act
        var result = await repository.UpdateAsync(nonExistentProduct);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("was not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Code = "P002", Category = "Cat", Price = 20.00m, Stock = 200 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        var productToDelete = new Product { Id = 1 };

        // Act
        await repository.DeleteAsync(productToDelete);

        // Assert
        var allProducts = await repository.GetAllAsync();
        Assert.Single(allProducts);
        Assert.Equal(2, allProducts[0].Id);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenEntityIsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.DeleteAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cannot be null", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenEntityNotFound()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        var nonExistentProduct = new Product { Id = 999 };

        // Act
        var result = await repository.DeleteAsync(nonExistentProduct);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("was not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyArray_WhenJsonIsInvalid()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "{ invalid json }");
        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyArray_WhenFileIsEmpty()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "");
        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Entities_ReturnsCollection()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        // Force loading
        await repository.GetAllAsync();

        // Act
        var entities = repository.Entities;

        // Assert
        Assert.NotNull(entities);
        Assert.Single(entities);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDataStoreIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ProductRepository(null!));
    }

    [Fact]
    public async Task SaveAsync_PersistsChangesToDataStore()
    {
        // Arrange
        var repository = CreateRepository();
        await repository.AddAsync(new Product { Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 });

        // Act
        var count = await repository.SaveAsync();

        // Assert
        Assert.Equal(1, count);
        Assert.True(File.Exists(_testFilePath));
        var content = await File.ReadAllTextAsync(_testFilePath);
        Assert.Contains("Test", content);
    }

    [Fact]
    public async Task ReloadAsync_RefreshesDataFromDataStore()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Original", Code = "O001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var repository = CreateRepository();

        // Initial load
        var initial = await repository.GetAllAsync();
        Assert.Single(initial);

        // Modify file externally
        var updatedProducts = new List<Product>
        {
            new() { Id = 1, Name = "Original", Code = "O001", Category = "Cat", Price = 10.00m, Stock = 100 },
            new() { Id = 2, Name = "New", Code = "N001", Category = "Cat", Price = 20.00m, Stock = 200 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(updatedProducts));

        // Act
        await repository.ReloadAsync();
        var reloaded = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, reloaded.Length);
    }
}
