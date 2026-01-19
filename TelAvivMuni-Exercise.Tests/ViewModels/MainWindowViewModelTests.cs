using Moq;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.ViewModels;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Product>> _mockProductRepository;

    public MainWindowViewModelTests()
    {
        _mockProductRepository = new Mock<IRepository<Product>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenUnitOfWorkIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(null!));
    }

    [Fact]
    public async Task Constructor_LoadsProducts()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = 1, Name = "Product 1", Code = "P001", Category = "Cat", Price = 10.00m, Stock = 100 },
            new Product { Id = 2, Name = "Product 2", Code = "P002", Category = "Cat", Price = 20.00m, Stock = 200 }
        };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        // Act
        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);

        // Give the async void method time to complete
        await Task.Delay(100);

        // Assert
        Assert.Equal(2, viewModel.Products.Count);
    }

    [Fact]
    public async Task Constructor_SetsEmptyProducts_WhenLoadFails()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);

        // Give the async void method time to complete
        await Task.Delay(100);

        // Assert
        Assert.Empty(viewModel.Products);
        Assert.Contains("Failed to load products", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task AddProductCommand_AddsAndSaves()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
        _mockProductRepository.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync(OperationResult.Ok());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        var newProduct = new Product { Id = 1, Name = "New", Code = "N001", Category = "Cat", Price = 15.00m, Stock = 10 };

        // Act
        viewModel.AddProductCommand.Execute(newProduct);
        await Task.Delay(100);

        // Assert
        _mockProductRepository.Verify(r => r.AddAsync(newProduct), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        Assert.Contains(newProduct, viewModel.Products);
    }

    [Fact]
    public async Task UpdateProductCommand_UpdatesAndSaves()
    {
        // Arrange
        var existingProduct = new Product { Id = 1, Name = "Original", Code = "O001", Category = "Cat", Price = 10.00m, Stock = 100 };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { existingProduct });
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(OperationResult.Ok());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        var updatedProduct = new Product { Id = 1, Name = "Updated", Code = "U001", Category = "Cat", Price = 25.00m, Stock = 50 };

        // Act
        viewModel.UpdateProductCommand.Execute(updatedProduct);
        await Task.Delay(100);

        // Assert
        _mockProductRepository.Verify(r => r.UpdateAsync(updatedProduct), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductCommand_DeletesAndSaves()
    {
        // Arrange
        var productToDelete = new Product { Id = 1, Name = "ToDelete", Code = "D001", Category = "Cat", Price = 10.00m, Stock = 100 };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { productToDelete });
        _mockProductRepository.Setup(r => r.DeleteAsync(It.IsAny<Product>())).ReturnsAsync(OperationResult.Ok());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        // Act
        viewModel.DeleteProductCommand.Execute(productToDelete);
        await Task.Delay(100);

        // Assert
        _mockProductRepository.Verify(r => r.DeleteAsync(productToDelete), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        Assert.DoesNotContain(productToDelete, viewModel.Products);
    }

    [Fact]
    public async Task SaveChangesCommand_CallsSaveChangesAsync()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        // Act
        viewModel.SaveChangesCommand.Execute(null);
        await Task.Delay(100);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SelectedProduct1_PropertyChangedNotification()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedProduct1))
                propertyChangedRaised = true;
        };

        var product = new Product { Id = 1, Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 };

        // Act
        viewModel.SelectedProduct1 = product;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(product, viewModel.SelectedProduct1);
    }

    [Fact]
    public async Task SelectedProduct2_PropertyChangedNotification()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedProduct2))
                propertyChangedRaised = true;
        };

        var product = new Product { Id = 1, Name = "Test", Code = "T001", Category = "Cat", Price = 10.00m, Stock = 100 };

        // Act
        viewModel.SelectedProduct2 = product;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(product, viewModel.SelectedProduct2);
    }

    [Fact]
    public async Task ErrorMessage_PropertyChangedNotification()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(100);

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.ErrorMessage))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal("Test error", viewModel.ErrorMessage);
    }
}
