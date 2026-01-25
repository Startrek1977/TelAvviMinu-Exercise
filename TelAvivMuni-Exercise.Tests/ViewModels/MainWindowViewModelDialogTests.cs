using System.Collections.ObjectModel;
using Moq;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.ViewModels;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.ViewModels;

/// <summary>
/// Tests for the dialog functionality in MainWindowViewModel (from MainWindowViewModel.Dialog.cs partial class).
/// These tests cover the PrepareDialog, Initialize, filtering, and command functionality.
/// </summary>
public class MainWindowViewModelDialogTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Product>> _mockProductRepository;

    public MainWindowViewModelDialogTests()
    {
        _mockProductRepository = new Mock<IRepository<Product>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
    }

    private async Task<MainWindowViewModel> CreateViewModelAsync()
    {
        var viewModel = new MainWindowViewModel(_mockUnitOfWork.Object);
        await Task.Delay(50); // Allow async initialization to complete
        return viewModel;
    }

    #region Test Data

    private static List<Product> CreateTestProducts() =>
    [
        new Product { Id = 1, Code = "P001", Name = "Laptop", Category = "Electronics", Price = 999.99m },
        new Product { Id = 2, Code = "P002", Name = "Mouse", Category = "Electronics", Price = 29.99m },
        new Product { Id = 3, Code = "P003", Name = "Desk", Category = "Furniture", Price = 299.99m },
        new Product { Id = 4, Code = "P004", Name = "Chair", Category = "Furniture", Price = 199.99m },
        new Product { Id = 5, Code = "P005", Name = "Monitor", Category = "Electronics", Price = 449.99m }
    ];

    #endregion

    #region PrepareDialog Tests

    [Fact]
    public async Task PrepareDialog_WithItems_SetsDialogIsLoadingTrue()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();

        // Act
        viewModel.PrepareDialog(CreateTestProducts(), null);

        // Assert
        Assert.True(viewModel.DialogIsLoading);
    }

    [Fact]
    public async Task PrepareDialog_WithItems_DialogItemsCountIsZeroBeforeInitialize()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();

        // Act
        viewModel.PrepareDialog(CreateTestProducts(), null);

        // Assert - Items are not loaded until Initialize() is called
        Assert.Equal(0, viewModel.DialogItemsCount);
    }

    [Fact]
    public async Task PrepareDialog_WithNullItems_DoesNotThrow()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();

        // Act
        var exception = Record.Exception(() => viewModel.PrepareDialog(null, null));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task PrepareDialog_WithColumns_SetsColumns()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var columns = new ObservableCollection<BrowserColumn>
        {
            new() { DataField = "Name", Header = "Name" },
            new() { DataField = "Price", Header = "Price" }
        };

        // Act
        viewModel.PrepareDialog(CreateTestProducts(), null, columns);

        // Assert
        Assert.Equal(columns, viewModel.Columns);
        Assert.True(viewModel.HasCustomColumns);
    }

    [Fact]
    public async Task PrepareDialog_WithoutColumns_HasCustomColumnsIsFalse()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();

        // Act
        viewModel.PrepareDialog(CreateTestProducts(), null);

        // Assert
        Assert.False(viewModel.HasCustomColumns);
    }

    [Fact]
    public async Task PrepareDialog_ResetsDialogState()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSearchText = "test";
        viewModel.DialogResult = true;

        // Act - Prepare dialog again
        viewModel.PrepareDialog(CreateTestProducts(), null);

        // Assert
        Assert.Equal(string.Empty, viewModel.DialogSearchText);
        Assert.Null(viewModel.DialogResult);
        Assert.True(viewModel.DialogIsLoading);
    }

    #endregion

    #region Initialize Tests (View-First Pattern)

    [Fact]
    public async Task Initialize_LoadsItemsIntoCollection()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var products = CreateTestProducts();
        viewModel.PrepareDialog(products, null);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.Equal(5, viewModel.DialogItemsCount);
    }

    [Fact]
    public async Task Initialize_SetsDialogIsLoadingFalse()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        Assert.True(viewModel.DialogIsLoading); // Verify precondition

        // Act
        viewModel.Initialize();

        // Assert
        Assert.False(viewModel.DialogIsLoading);
    }

    [Fact]
    public async Task Initialize_WithCurrentSelection_SelectsMatchingItem()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var products = CreateTestProducts();
        var selectedProduct = products[2]; // Desk
        viewModel.PrepareDialog(products, selectedProduct);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.NotNull(viewModel.DialogSelectedItem);
        Assert.Equal(selectedProduct.Id, ((Product)viewModel.DialogSelectedItem).Id);
    }

    [Fact]
    public async Task Initialize_WithCurrentSelectionById_SelectsMatchingItem()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var products = CreateTestProducts();
        // Create a different instance with the same Id to test Id-based matching
        var selectionWithSameId = new Product { Id = 3, Code = "P003", Name = "Desk", Category = "Furniture", Price = 299.99m };
        viewModel.PrepareDialog(products, selectionWithSameId);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.NotNull(viewModel.DialogSelectedItem);
        Assert.Equal(3, ((Product)viewModel.DialogSelectedItem).Id);
    }

    [Fact]
    public async Task Initialize_WithNullSelection_DialogSelectedItemIsNull()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.Null(viewModel.DialogSelectedItem);
    }

    [Fact]
    public async Task Initialize_WithNullItems_DoesNotThrow()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(null, null);

        // Act
        var exception = Record.Exception(() => viewModel.Initialize());

        // Assert
        Assert.Null(exception);
        Assert.Equal(0, viewModel.DialogItemsCount);
        Assert.False(viewModel.DialogIsLoading);
    }

    [Fact]
    public async Task Initialize_CalledTwice_DoesNotDuplicateItems()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var products = CreateTestProducts();
        viewModel.PrepareDialog(products, null);

        // Act
        viewModel.Initialize();
        viewModel.Initialize(); // Second call should be no-op

        // Assert
        Assert.Equal(5, viewModel.DialogItemsCount);
    }

    #endregion

    #region IDeferredInitialization Interface Tests

    [Fact]
    public async Task ViewModel_ImplementsIDeferredInitialization()
    {
        // Arrange & Act
        var viewModel = await CreateViewModelAsync();

        // Assert
        Assert.IsAssignableFrom<IDeferredInitialization>(viewModel);
    }

    #endregion

    #region IColumnConfiguration Interface Tests

    [Fact]
    public async Task ViewModel_ImplementsIColumnConfiguration()
    {
        // Arrange & Act
        var viewModel = await CreateViewModelAsync();

        // Assert
        Assert.IsAssignableFrom<IColumnConfiguration>(viewModel);
    }

    [Fact]
    public async Task IColumnConfiguration_Columns_ReturnsConfiguredColumns()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var columns = new ObservableCollection<BrowserColumn>
        {
            new() { DataField = "Name", Header = "Product Name" }
        };
        viewModel.PrepareDialog(CreateTestProducts(), null, columns);

        // Act & Assert
        IColumnConfiguration columnConfig = viewModel;
        Assert.Same(columns, columnConfig.Columns);
    }

    [Fact]
    public async Task IColumnConfiguration_HasCustomColumns_ReturnsTrueWhenColumnsExist()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var columns = new ObservableCollection<BrowserColumn>
        {
            new() { DataField = "Name", Header = "Product Name" }
        };
        viewModel.PrepareDialog(CreateTestProducts(), null, columns);

        // Assert
        IColumnConfiguration columnConfig = viewModel;
        Assert.True(columnConfig.HasCustomColumns);
    }

    [Fact]
    public async Task IColumnConfiguration_HasCustomColumns_ReturnsFalseWhenNoColumns()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);

        // Assert
        IColumnConfiguration columnConfig = viewModel;
        Assert.False(columnConfig.HasCustomColumns);
    }

    [Fact]
    public async Task IColumnConfiguration_HasCustomColumns_ReturnsFalseWhenEmptyColumns()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var columns = new ObservableCollection<BrowserColumn>();
        viewModel.PrepareDialog(CreateTestProducts(), null, columns);

        // Assert
        IColumnConfiguration columnConfig = viewModel;
        Assert.False(columnConfig.HasCustomColumns);
    }

    #endregion

    #region Search/Filter Tests

    [Fact]
    public async Task DialogSearchText_FiltersByProductName()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "Laptop";

        // Assert
        Assert.Equal(1, viewModel.DialogItemsCount);
    }

    [Fact]
    public async Task DialogSearchText_FiltersByCategory()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "Electronics";

        // Assert
        Assert.Equal(3, viewModel.DialogItemsCount); // Laptop, Mouse, Monitor
    }

    [Fact]
    public async Task DialogSearchText_CaseInsensitive()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "laptop";

        // Assert
        Assert.Equal(1, viewModel.DialogItemsCount);
    }

    [Fact]
    public async Task DialogSearchText_EmptyString_ShowsAllItems()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSearchText = "Electronics";

        // Act
        viewModel.DialogSearchText = string.Empty;

        // Assert
        Assert.Equal(5, viewModel.DialogItemsCount);
    }

    [Fact]
    public async Task DialogHasSearchText_TrueWhenTextEntered()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "test";

        // Assert
        Assert.True(viewModel.DialogHasSearchText);
    }

    [Fact]
    public async Task DialogHasSearchText_FalseWhenEmpty()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Assert
        Assert.False(viewModel.DialogHasSearchText);
    }

    [Fact]
    public async Task DialogHasSearchText_FalseWhenWhitespace()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "   ";

        // Assert
        Assert.False(viewModel.DialogHasSearchText);
    }

    #endregion

    #region Selection Tests

    [Fact]
    public async Task DialogSelectedItem_CanBeSetAndRetrieved()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var itemToSelect = viewModel.DialogFilteredItems!.Cast<object>().First();

        // Act
        viewModel.DialogSelectedItem = itemToSelect;

        // Assert
        Assert.Same(itemToSelect, viewModel.DialogSelectedItem);
    }

    [Fact]
    public async Task DialogSelectedItem_CanBeCleared()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var products = CreateTestProducts();
        viewModel.PrepareDialog(products, products[0]);
        viewModel.Initialize();

        // Act
        viewModel.DialogSelectedItem = null;

        // Assert
        Assert.Null(viewModel.DialogSelectedItem);
    }

    #endregion

    #region Command Tests

    [Fact]
    public async Task DialogOkCommand_SetsDialogResultTrue()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSelectedItem = viewModel.DialogFilteredItems!.Cast<object>().First();

        // Act
        viewModel.DialogOkCommand.Execute(null);

        // Assert
        Assert.True(viewModel.DialogResult);
    }

    [Fact]
    public async Task DialogOkCommand_CanExecute_FalseWhenNoSelection()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Assert
        Assert.False(viewModel.DialogOkCommand.CanExecute(null));
    }

    [Fact]
    public async Task DialogOkCommand_CanExecute_TrueWhenItemSelected()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSelectedItem = viewModel.DialogFilteredItems!.Cast<object>().First();

        // Assert
        Assert.True(viewModel.DialogOkCommand.CanExecute(null));
    }

    [Fact]
    public async Task DialogOkCommand_CanExecute_FalseWhenSelectedItemFilteredOut()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSelectedItem = viewModel.DialogFilteredItems!.Cast<object>().First(); // Laptop

        // Act - Filter to show only Furniture
        viewModel.DialogSearchText = "Furniture";

        // Assert - Laptop is filtered out, so OK should be disabled
        Assert.False(viewModel.DialogOkCommand.CanExecute(null));
    }

    [Fact]
    public async Task DialogCancelCommand_SetsDialogResultFalse()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogCancelCommand.Execute(null);

        // Assert
        Assert.False(viewModel.DialogResult);
    }

    [Fact]
    public async Task DialogClearSearchCommand_ClearsDialogSearchText()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSearchText = "test search";

        // Act
        viewModel.DialogClearSearchCommand.Execute(null);

        // Assert
        Assert.Equal(string.Empty, viewModel.DialogSearchText);
    }

    #endregion

    #region DialogFilteredItems Tests

    [Fact]
    public async Task DialogFilteredItems_ReturnsAllItemsWhenNoFilter()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Assert
        Assert.Equal(5, viewModel.DialogFilteredItems!.Cast<object>().Count());
    }

    [Fact]
    public async Task DialogFilteredItems_ReturnsFilteredItemsWhenSearchApplied()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "Chair";

        // Assert
        var filteredItems = viewModel.DialogFilteredItems!.Cast<Product>().ToList();
        Assert.Single(filteredItems);
        Assert.Equal("Chair", filteredItems[0].Name);
    }

    #endregion

    #region FindMatchingItem Tests (Additional Coverage)

    [Fact]
    public async Task Initialize_WithNonEntitySelection_UsesEqualsComparison()
    {
        // Arrange - using string values that implement Equals
        var viewModel = await CreateViewModelAsync();
        var items = new List<string> { "Apple", "Banana", "Cherry" };
        var selection = "Banana"; // Same value, could be different reference
        viewModel.PrepareDialog(items, selection);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.Equal("Banana", viewModel.DialogSelectedItem);
    }

    [Fact]
    public async Task Initialize_WithReferenceEqualSelection_MatchesByReference()
    {
        // Arrange - use the exact same reference from the list
        var viewModel = await CreateViewModelAsync();
        var items = new List<object> { new object(), new object(), new object() };
        var selection = items[1]; // Same reference
        viewModel.PrepareDialog(items, selection);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.Same(selection, viewModel.DialogSelectedItem);
    }

    [Fact]
    public async Task Initialize_WithNonEntityNonEqualsSelection_UsesReferenceEqualityFallback()
    {
        // Arrange - custom class that doesn't implement IEntity and doesn't override Equals
        var viewModel = await CreateViewModelAsync();
        var item1 = new CustomNonEntity { Value = "A" };
        var item2 = new CustomNonEntity { Value = "B" };
        var items = new List<CustomNonEntity> { item1, item2 };

        // Pass the exact same reference - should match by reference equality fallback
        viewModel.PrepareDialog(items, item1);

        // Act
        viewModel.Initialize();

        // Assert - Should find item1 via reference equality in FindMatchingDialogItem
        Assert.Same(item1, viewModel.DialogSelectedItem);
    }

    [Fact]
    public async Task Initialize_WithNonEntityNonMatchingSelection_ReturnsNullViaAllFallbacks()
    {
        // Arrange - create items and a selection that won't match by IEntity.Id, Equals, or ReferenceEquals
        var viewModel = await CreateViewModelAsync();
        var items = new List<CustomNonEntity>
        {
            new CustomNonEntity { Value = "A" },
            new CustomNonEntity { Value = "B" }
        };
        // Create a completely different instance - no IEntity, no Equals override, different reference
        var selection = new CustomNonEntity { Value = "A" }; // Different instance, not reference equal
        viewModel.PrepareDialog(items, selection);

        // Act
        viewModel.Initialize();

        // Assert - None of the matching strategies work, so null is returned
        Assert.Null(viewModel.DialogSelectedItem);
    }

    // Helper class that doesn't implement IEntity and uses default Equals (reference equality)
    private class CustomNonEntity
    {
        public string Value { get; set; } = string.Empty;
        // Deliberately NOT overriding Equals - uses default reference equality
    }

    [Fact]
    public async Task Initialize_WithNonMatchingSelection_DialogSelectedItemIsNull()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        var products = CreateTestProducts();
        var nonExistingProduct = new Product { Id = 999, Code = "P999", Name = "NonExistent", Category = "None", Price = 0m };
        viewModel.PrepareDialog(products, nonExistingProduct);

        // Act
        viewModel.Initialize();

        // Assert
        Assert.Null(viewModel.DialogSelectedItem);
    }

    #endregion

    #region PropertyChanged Notification Tests

    [Fact]
    public async Task DialogSearchText_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DialogSearchText))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DialogSearchText = "test";

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public async Task DialogSearchText_RaisesDialogHasSearchTextPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DialogHasSearchText))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DialogSearchText = "test";

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public async Task DialogSearchText_RaisesDialogItemsCountPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DialogItemsCount))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DialogSearchText = "Electronics";

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public async Task DialogIsLoading_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DialogIsLoading))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.Initialize();

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public async Task DialogResult_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DialogResult))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DialogResult = true;

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public async Task DialogSelectedItem_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.DialogSelectedItem))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DialogSelectedItem = viewModel.DialogFilteredItems!.Cast<object>().First();

        // Assert
        Assert.True(propertyChangedRaised);
    }

    #endregion

    #region Filter With Selection Preservation Tests

    [Fact]
    public async Task DialogSearchText_PreservesSelectionWhenItemRemainsVisible()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var laptop = viewModel.DialogFilteredItems!.Cast<Product>().First(p => p.Name == "Laptop");
        viewModel.DialogSelectedItem = laptop;

        // Act - Filter to Electronics (Laptop should remain visible)
        viewModel.DialogSearchText = "Electronics";

        // Assert
        Assert.Same(laptop, viewModel.DialogSelectedItem);
        Assert.True(viewModel.DialogOkCommand.CanExecute(null));
    }

    [Fact]
    public async Task DialogSearchText_WhenSelectedItemFilteredOut_RaisesSelectedItemPropertyChanged()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var laptop = viewModel.DialogFilteredItems!.Cast<Product>().First(p => p.Name == "Laptop");
        viewModel.DialogSelectedItem = laptop;

        // Act - Filter to Furniture (Laptop should be filtered out)
        viewModel.DialogSearchText = "Furniture";

        // Assert - DialogSelectedItem still holds the reference but it's not visible
        Assert.Same(laptop, viewModel.DialogSelectedItem);
        Assert.False(viewModel.DialogOkCommand.CanExecute(null)); // Can't OK when selection is filtered out
    }

    [Fact]
    public async Task ClearSearch_RestoresSelectedItemVisibility()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        var laptop = viewModel.DialogFilteredItems!.Cast<Product>().First(p => p.Name == "Laptop");
        viewModel.DialogSelectedItem = laptop;
        viewModel.DialogSearchText = "Furniture"; // Filter out Laptop
        Assert.False(viewModel.DialogOkCommand.CanExecute(null));

        // Act - Clear search
        viewModel.DialogClearSearchCommand.Execute(null);

        // Assert - Laptop should be visible again
        Assert.True(viewModel.DialogOkCommand.CanExecute(null));
    }

    #endregion

    #region Filter Edge Cases

    [Fact]
    public async Task DialogSearchText_FiltersByProductCode()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act - Search by code
        viewModel.DialogSearchText = "P003";

        // Assert
        Assert.Equal(1, viewModel.DialogItemsCount);
        var filteredProduct = viewModel.DialogFilteredItems!.Cast<Product>().First();
        Assert.Equal("Desk", filteredProduct.Name);
    }

    [Fact]
    public async Task DialogSearchText_FiltersByPrice()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act - Search by price
        viewModel.DialogSearchText = "999.99";

        // Assert
        Assert.Equal(1, viewModel.DialogItemsCount);
        var filteredProduct = viewModel.DialogFilteredItems!.Cast<Product>().First();
        Assert.Equal("Laptop", filteredProduct.Name);
    }

    [Fact]
    public async Task DialogSearchText_PartialMatch_ReturnsMatchingItems()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act - Partial search
        viewModel.DialogSearchText = "tron"; // Matches "Electronics"

        // Assert
        Assert.Equal(3, viewModel.DialogItemsCount); // Laptop, Mouse, Monitor
    }

    [Fact]
    public async Task DialogSearchText_NoMatches_ReturnsEmptyCollection()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();

        // Act
        viewModel.DialogSearchText = "XYZ123NonExistent";

        // Assert
        Assert.Equal(0, viewModel.DialogItemsCount);
    }

    #endregion

    #region Command CanExecute Edge Cases

    [Fact]
    public async Task DialogOkCommand_AfterSettingSameSearchText_DoesNotDuplicateNotifications()
    {
        // Arrange
        var viewModel = await CreateViewModelAsync();
        viewModel.PrepareDialog(CreateTestProducts(), null);
        viewModel.Initialize();
        viewModel.DialogSelectedItem = viewModel.DialogFilteredItems!.Cast<object>().First();
        viewModel.DialogSearchText = "test";

        // Act - Set same search text again
        viewModel.DialogSearchText = "test";

        // Assert - Should still work without issues
        Assert.False(viewModel.DialogOkCommand.CanExecute(null)); // No items match "test"
    }

    #endregion
}
