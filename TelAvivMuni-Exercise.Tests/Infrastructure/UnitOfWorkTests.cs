using System.IO;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Infrastructure;

public class UnitOfWorkTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testJsonPath;

    public UnitOfWorkTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"UoWTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _testJsonPath = Path.Combine(_testDirectory, "Products.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void Products_ReturnsRepository()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork();

        // Act
        var products = unitOfWork.Products;

        // Assert
        Assert.NotNull(products);
        Assert.IsAssignableFrom<IRepository<Product>>(products);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsCount()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork();

        // Load and save without changes
        await unitOfWork.Products.GetAllAsync();

        // Act
        var result = await unitOfWork.SaveChangesAsync();

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var unitOfWork = new UnitOfWork();

        // Act & Assert - should not throw
        unitOfWork.Dispose();
        unitOfWork.Dispose();
        unitOfWork.Dispose();
    }

    [Fact]
    public void Products_SameInstance_MultipleCalls()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork();

        // Act
        var products1 = unitOfWork.Products;
        var products2 = unitOfWork.Products;

        // Assert
        Assert.Same(products1, products2);
    }

    [Fact]
    public async Task IntegrationTest_AddAndSave()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork();
        var product = new Product
        {
            Id = 0,
            Name = "Test Product",
            Code = "TP001",
            Category = "Test",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        await unitOfWork.Products.AddAsync(product);
        var saveResult = await unitOfWork.SaveChangesAsync();

        // Assert
        Assert.True(saveResult >= 0);
        Assert.NotEqual(0, product.Id);
    }
}
