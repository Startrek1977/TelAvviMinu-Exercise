using System.IO;
using System.Text.Json;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Infrastructure;

public class FileDataStoreTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFilePath;

    public FileDataStoreTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"FileDataStoreTests_{Guid.NewGuid()}");
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

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFilePathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileDataStore<Product>(null!, new JsonSerializer<Product>()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSerializerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileDataStore<Product>(_testFilePath, null!));
    }

    [Fact]
    public void FilePath_ReturnsCorrectPath()
    {
        // Arrange
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());

        // Act & Assert
        Assert.Equal(_testFilePath, dataStore.FilePath);
    }

    [Fact]
    public async Task LoadAsync_ReturnsEmptyArray_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent.json");
        var dataStore = new FileDataStore<Product>(nonExistentPath, new JsonSerializer<Product>());

        // Act
        var result = await dataStore.LoadAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadAsync_ReturnsEntities_WhenFileExists()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Code = "P002", Category = "Cat", Price = 20.00m, Stock = 200 }
        };
        await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(products));
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());

        // Act
        var result = await dataStore.LoadAsync();

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("Product 1", result[0].Name);
        Assert.Equal("Product 2", result[1].Name);
    }

    [Fact]
    public async Task SaveAsync_WritesDataToFile()
    {
        // Arrange
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };

        // Act
        var count = await dataStore.SaveAsync(products);

        // Assert
        Assert.Equal(1, count);
        Assert.True(File.Exists(_testFilePath));
        var content = await File.ReadAllTextAsync(_testFilePath);
        Assert.Contains("Test", content);
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectory_WhenNotExists()
    {
        // Arrange
        var newDirectory = Path.Combine(_testDirectory, "NewSubDir");
        var filePath = Path.Combine(newDirectory, "Products.json");
        var dataStore = new FileDataStore<Product>(filePath, new JsonSerializer<Product>());
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };

        // Act
        await dataStore.SaveAsync(products);

        // Assert
        Assert.True(Directory.Exists(newDirectory));
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task SaveAsync_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Arrange
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => dataStore.SaveAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_ReturnsEmptyArray_WhenFileIsEmpty()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "");
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());

        // Act
        var result = await dataStore.LoadAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadAsync_ReturnsEmptyArray_WhenJsonIsInvalid()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "{ invalid json }");
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());

        // Act
        var result = await dataStore.LoadAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        // Arrange
        var dataStore = new FileDataStore<Product>(_testFilePath, new JsonSerializer<Product>());

        // Save initial data
        var initialProducts = new List<Product>
        {
            new() { Id = 1, Name = "Initial", Code = "I001", Category = "Cat", Price = 5.00m, Stock = 50 }
        };
        await dataStore.SaveAsync(initialProducts);

        // Save new data
        var newProducts = new List<Product>
        {
            new() { Id = 2, Name = "New", Code = "N001", Category = "Cat", Price = 10.00m, Stock = 100 }
        };

        // Act
        await dataStore.SaveAsync(newProducts);

        // Assert
        var loaded = await dataStore.LoadAsync();
        Assert.Single(loaded);
        Assert.Equal("New", loaded[0].Name);
        Assert.Equal(2, loaded[0].Id);
    }
}
