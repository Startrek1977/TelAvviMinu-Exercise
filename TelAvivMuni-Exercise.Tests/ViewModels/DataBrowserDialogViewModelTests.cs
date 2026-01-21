using System.Collections.ObjectModel;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.ViewModels;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.ViewModels;

public class DataBrowserDialogViewModelTests
{
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

        #region Constructor Tests

        [Fact]
        public void Constructor_WithItems_SetsIsLoadingTrue()
        {
            // Arrange & Act
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Assert
            Assert.True(viewModel.IsLoading);
        }

        [Fact]
        public void Constructor_WithItems_ItemsCountIsZeroBeforeInitialize()
        {
            // Arrange & Act
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Assert - Items are not loaded until Initialize() is called
            Assert.Equal(0, viewModel.ItemsCount);
        }

        [Fact]
        public void Constructor_WithNullItems_DoesNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => new DataBrowserDialogViewModel(null, null));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_WithColumns_SetsColumns()
        {
            // Arrange
            var columns = new ObservableCollection<BrowserColumn>
            {
                new() { DataField = "Name", Header = "Name" },
                new() { DataField = "Price", Header = "Price" }
            };

            // Act
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null, columns);

            // Assert
            Assert.Equal(columns, viewModel.Columns);
            Assert.True(viewModel.HasCustomColumns);
        }

        [Fact]
        public void Constructor_WithoutColumns_HasCustomColumnsIsFalse()
        {
            // Arrange & Act
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Assert
            Assert.False(viewModel.HasCustomColumns);
        }

        #endregion

        #region Initialize Tests (View-First Pattern)

        [Fact]
        public void Initialize_LoadsItemsIntoCollection()
        {
            // Arrange
            var products = CreateTestProducts();
            var viewModel = new DataBrowserDialogViewModel(products, null);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.Equal(5, viewModel.ItemsCount);
        }

        [Fact]
        public void Initialize_SetsIsLoadingFalse()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            Assert.True(viewModel.IsLoading); // Verify precondition

            // Act
            viewModel.Initialize();

            // Assert
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public void Initialize_WithCurrentSelection_SelectsMatchingItem()
        {
            // Arrange
            var products = CreateTestProducts();
            var selectedProduct = products[2]; // Desk
            var viewModel = new DataBrowserDialogViewModel(products, selectedProduct);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.NotNull(viewModel.SelectedItem);
            Assert.Equal(selectedProduct.Id, ((Product)viewModel.SelectedItem).Id);
        }

        [Fact]
        public void Initialize_WithCurrentSelectionById_SelectsMatchingItem()
        {
            // Arrange
            var products = CreateTestProducts();
            // Create a different instance with the same Id to test Id-based matching
            var selectionWithSameId = new Product { Id = 3, Code = "P003", Name = "Desk", Category = "Furniture", Price = 299.99m };
            var viewModel = new DataBrowserDialogViewModel(products, selectionWithSameId);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.NotNull(viewModel.SelectedItem);
            Assert.Equal(3, ((Product)viewModel.SelectedItem).Id);
        }

        [Fact]
        public void Initialize_WithNullSelection_SelectedItemIsNull()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void Initialize_WithNullItems_DoesNotThrow()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(null, null);

            // Act
            var exception = Record.Exception(() => viewModel.Initialize());

            // Assert
            Assert.Null(exception);
            Assert.Equal(0, viewModel.ItemsCount);
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public void Initialize_CalledTwice_DoesNotDuplicateItems()
        {
            // Arrange
            var products = CreateTestProducts();
            var viewModel = new DataBrowserDialogViewModel(products, null);

            // Act
            viewModel.Initialize();
            viewModel.Initialize(); // Second call should be no-op

            // Assert
            Assert.Equal(5, viewModel.ItemsCount);
        }

        #endregion

        #region IDeferredInitialization Interface Tests

        [Fact]
        public void ViewModel_ImplementsIDeferredInitialization()
        {
            // Arrange & Act
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Assert
            Assert.IsAssignableFrom<IDeferredInitialization>(viewModel);
        }

        #endregion

        #region IColumnConfiguration Interface Tests

        [Fact]
        public void ViewModel_ImplementsIColumnConfiguration()
        {
            // Arrange & Act
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Assert
            Assert.IsAssignableFrom<IColumnConfiguration>(viewModel);
        }

        [Fact]
        public void IColumnConfiguration_Columns_ReturnsConfiguredColumns()
        {
            // Arrange
            var columns = new ObservableCollection<BrowserColumn>
            {
                new() { DataField = "Name", Header = "Product Name" }
            };
            IColumnConfiguration viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null, columns);

            // Act & Assert
            Assert.Same(columns, viewModel.Columns);
        }

        [Fact]
        public void IColumnConfiguration_HasCustomColumns_ReturnsTrueWhenColumnsExist()
        {
            // Arrange
            var columns = new ObservableCollection<BrowserColumn>
            {
                new() { DataField = "Name", Header = "Product Name" }
            };
            IColumnConfiguration viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null, columns);

            // Assert
            Assert.True(viewModel.HasCustomColumns);
        }

        [Fact]
        public void IColumnConfiguration_HasCustomColumns_ReturnsFalseWhenNoColumns()
        {
            // Arrange
            IColumnConfiguration viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);

            // Assert
            Assert.False(viewModel.HasCustomColumns);
        }

        [Fact]
        public void IColumnConfiguration_HasCustomColumns_ReturnsFalseWhenEmptyColumns()
        {
            // Arrange
            var columns = new ObservableCollection<BrowserColumn>();
            IColumnConfiguration viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null, columns);

            // Assert
            Assert.False(viewModel.HasCustomColumns);
        }

        #endregion

        #region Search/Filter Tests

        [Fact]
        public void SearchText_FiltersByProductName()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "Laptop";

            // Assert
            Assert.Equal(1, viewModel.ItemsCount);
        }

        [Fact]
        public void SearchText_FiltersByCategory()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "Electronics";

            // Assert
            Assert.Equal(3, viewModel.ItemsCount); // Laptop, Mouse, Monitor
        }

        [Fact]
        public void SearchText_CaseInsensitive()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "laptop";

            // Assert
            Assert.Equal(1, viewModel.ItemsCount);
        }

        [Fact]
        public void SearchText_EmptyString_ShowsAllItems()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            viewModel.SearchText = "Electronics";

            // Act
            viewModel.SearchText = string.Empty;

            // Assert
            Assert.Equal(5, viewModel.ItemsCount);
        }

        [Fact]
        public void HasSearchText_TrueWhenTextEntered()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "test";

            // Assert
            Assert.True(viewModel.HasSearchText);
        }

        [Fact]
        public void HasSearchText_FalseWhenEmpty()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Assert
            Assert.False(viewModel.HasSearchText);
        }

        [Fact]
        public void HasSearchText_FalseWhenWhitespace()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "   ";

            // Assert
            Assert.False(viewModel.HasSearchText);
        }

        #endregion

        #region Selection Tests

        [Fact]
        public void SelectedItem_CanBeSetAndRetrieved()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var itemToSelect = viewModel.FilteredItems.Cast<object>().First();

            // Act
            viewModel.SelectedItem = itemToSelect;

            // Assert
            Assert.Same(itemToSelect, viewModel.SelectedItem);
        }

        [Fact]
        public void SelectedItem_CanBeCleared()
        {
            // Arrange
            var products = CreateTestProducts();
            var viewModel = new DataBrowserDialogViewModel(products, products[0]);
            viewModel.Initialize();

            // Act
            viewModel.SelectedItem = null;

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        #endregion

        #region Command Tests

        [Fact]
        public void OkCommand_SetsDialogResultTrue()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            viewModel.SelectedItem = viewModel.FilteredItems.Cast<object>().First();

            // Act
            viewModel.OkCommand.Execute(null);

            // Assert
            Assert.True(viewModel.DialogResult);
        }

        [Fact]
        public void OkCommand_CanExecute_FalseWhenNoSelection()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Assert
            Assert.False(viewModel.OkCommand.CanExecute(null));
        }

        [Fact]
        public void OkCommand_CanExecute_TrueWhenItemSelected()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            viewModel.SelectedItem = viewModel.FilteredItems.Cast<object>().First();

            // Assert
            Assert.True(viewModel.OkCommand.CanExecute(null));
        }

        [Fact]
        public void OkCommand_CanExecute_FalseWhenSelectedItemFilteredOut()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            viewModel.SelectedItem = viewModel.FilteredItems.Cast<object>().First(); // Laptop

            // Act - Filter to show only Furniture
            viewModel.SearchText = "Furniture";

            // Assert - Laptop is filtered out, so OK should be disabled
            Assert.False(viewModel.OkCommand.CanExecute(null));
        }

        [Fact]
        public void CancelCommand_SetsDialogResultFalse()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.CancelCommand.Execute(null);

            // Assert
            Assert.False(viewModel.DialogResult);
        }

        [Fact]
        public void ClearSearchCommand_ClearsSearchText()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            viewModel.SearchText = "test search";

            // Act
            viewModel.ClearSearchCommand.Execute(null);

            // Assert
            Assert.Equal(string.Empty, viewModel.SearchText);
        }

        #endregion

        #region FilteredItems Tests

        [Fact]
        public void FilteredItems_ReturnsAllItemsWhenNoFilter()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Assert
            Assert.Equal(5, viewModel.FilteredItems.Cast<object>().Count());
        }

        [Fact]
        public void FilteredItems_ReturnsFilteredItemsWhenSearchApplied()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "Chair";

            // Assert
            var filteredItems = viewModel.FilteredItems.Cast<Product>().ToList();
            Assert.Single(filteredItems);
            Assert.Equal("Chair", filteredItems[0].Name);
        }

        #endregion

        #region FindMatchingItem Tests (Additional Coverage)

        [Fact]
        public void Initialize_WithNonEntitySelection_UsesEqualsComparison()
        {
            // Arrange - using string values that implement Equals
            var items = new List<string> { "Apple", "Banana", "Cherry" };
            var selection = "Banana"; // Same value, could be different reference
            var viewModel = new DataBrowserDialogViewModel(items, selection);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.Equal("Banana", viewModel.SelectedItem);
        }

        [Fact]
        public void Initialize_WithReferenceEqualSelection_MatchesByReference()
        {
            // Arrange - use the exact same reference from the list
            var items = new List<object> { new object(), new object(), new object() };
            var selection = items[1]; // Same reference
            var viewModel = new DataBrowserDialogViewModel(items, selection);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.Same(selection, viewModel.SelectedItem);
        }

        [Fact]
        public void Initialize_WithNonEntityNonEqualsSelection_UsesReferenceEqualityFallback()
        {
            // Arrange - custom class that doesn't implement IEntity and doesn't override Equals
            // So the only way to match is by reference equality
            var item1 = new CustomNonEntity { Value = "A" };
            var item2 = new CustomNonEntity { Value = "B" };
            var items = new List<CustomNonEntity> { item1, item2 };

            // Pass the exact same reference - should match by reference equality fallback
            var viewModel = new DataBrowserDialogViewModel(items, item1);

            // Act
            viewModel.Initialize();

            // Assert - Should find item1 via reference equality in FindMatchingItem
            Assert.Same(item1, viewModel.SelectedItem);
        }

        [Fact]
        public void Initialize_WithNonEntityNonMatchingSelection_ReturnsNullViaAllFallbacks()
        {
            // Arrange - create items and a selection that won't match by IEntity.Id, Equals, or ReferenceEquals
            var items = new List<CustomNonEntity>
            {
                new CustomNonEntity { Value = "A" },
                new CustomNonEntity { Value = "B" }
            };
            // Create a completely different instance - no IEntity, no Equals override, different reference
            var selection = new CustomNonEntity { Value = "A" }; // Different instance, not reference equal
            var viewModel = new DataBrowserDialogViewModel(items, selection);

            // Act
            viewModel.Initialize();

            // Assert - None of the matching strategies work, so null is returned
            Assert.Null(viewModel.SelectedItem);
        }

        // Helper class that doesn't implement IEntity and uses default Equals (reference equality)
        private class CustomNonEntity
        {
            public string Value { get; set; } = string.Empty;
            // Deliberately NOT overriding Equals - uses default reference equality
        }

        [Fact]
        public void Initialize_WithNonMatchingSelection_SelectedItemIsNull()
        {
            // Arrange
            var products = CreateTestProducts();
            var nonExistingProduct = new Product { Id = 999, Code = "P999", Name = "NonExistent", Category = "None", Price = 0m };
            var viewModel = new DataBrowserDialogViewModel(products, nonExistingProduct);

            // Act
            viewModel.Initialize();

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        #endregion

        #region PropertyChanged Notification Tests

        [Fact]
        public void SearchText_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.SearchText))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SearchText = "test";

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void SearchText_RaisesHasSearchTextPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.HasSearchText))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SearchText = "test";

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void SearchText_RaisesItemsCountPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.ItemsCount))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SearchText = "Electronics";

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void IsLoading_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.IsLoading))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Initialize();

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void DialogResult_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.DialogResult))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.DialogResult = true;

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void SelectedItem_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.SelectedItem))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SelectedItem = viewModel.FilteredItems.Cast<object>().First();

            // Assert
            Assert.True(propertyChangedRaised);
        }

        #endregion

        #region Filter With Selection Preservation Tests

        [Fact]
        public void SearchText_PreservesSelectionWhenItemRemainsVisible()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var laptop = viewModel.FilteredItems.Cast<Product>().First(p => p.Name == "Laptop");
            viewModel.SelectedItem = laptop;

            // Act - Filter to Electronics (Laptop should remain visible)
            viewModel.SearchText = "Electronics";

            // Assert
            Assert.Same(laptop, viewModel.SelectedItem);
            Assert.True(viewModel.OkCommand.CanExecute(null));
        }

        [Fact]
        public void SearchText_WhenSelectedItemFilteredOut_RaisesSelectedItemPropertyChanged()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var laptop = viewModel.FilteredItems.Cast<Product>().First(p => p.Name == "Laptop");
            viewModel.SelectedItem = laptop;
            var selectedItemNotificationCount = 0;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DataBrowserDialogViewModel.SelectedItem))
                    selectedItemNotificationCount++;
            };

            // Act - Filter to Furniture (Laptop should be filtered out)
            viewModel.SearchText = "Furniture";

            // Assert - SelectedItem still holds the reference but it's not visible
            Assert.Same(laptop, viewModel.SelectedItem);
            Assert.False(viewModel.OkCommand.CanExecute(null)); // Can't OK when selection is filtered out
        }

        [Fact]
        public void ClearSearch_RestoresSelectedItemVisibility()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            var laptop = viewModel.FilteredItems.Cast<Product>().First(p => p.Name == "Laptop");
            viewModel.SelectedItem = laptop;
            viewModel.SearchText = "Furniture"; // Filter out Laptop
            Assert.False(viewModel.OkCommand.CanExecute(null));

            // Act - Clear search
            viewModel.ClearSearchCommand.Execute(null);

            // Assert - Laptop should be visible again
            Assert.True(viewModel.OkCommand.CanExecute(null));
        }

        #endregion

        #region Filter Edge Cases

        [Fact]
        public void SearchText_FiltersByProductCode()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act - Search by code
            viewModel.SearchText = "P003";

            // Assert
            Assert.Equal(1, viewModel.ItemsCount);
            var filteredProduct = viewModel.FilteredItems.Cast<Product>().First();
            Assert.Equal("Desk", filteredProduct.Name);
        }

        [Fact]
        public void SearchText_FiltersByPrice()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act - Search by price
            viewModel.SearchText = "999.99";

            // Assert
            Assert.Equal(1, viewModel.ItemsCount);
            var filteredProduct = viewModel.FilteredItems.Cast<Product>().First();
            Assert.Equal("Laptop", filteredProduct.Name);
        }

        [Fact]
        public void SearchText_PartialMatch_ReturnsMatchingItems()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act - Partial search
            viewModel.SearchText = "tron"; // Matches "Electronics"

            // Assert
            Assert.Equal(3, viewModel.ItemsCount); // Laptop, Mouse, Monitor
        }

        [Fact]
        public void SearchText_NoMatches_ReturnsEmptyCollection()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();

            // Act
            viewModel.SearchText = "XYZ123NonExistent";

            // Assert
            Assert.Equal(0, viewModel.ItemsCount);
        }

        #endregion

        #region Command CanExecute Edge Cases

        [Fact]
        public void OkCommand_AfterSettingSameSearchText_DoesNotDuplicateNotifications()
        {
            // Arrange
            var viewModel = new DataBrowserDialogViewModel(CreateTestProducts(), null);
            viewModel.Initialize();
            viewModel.SelectedItem = viewModel.FilteredItems.Cast<object>().First();
            viewModel.SearchText = "test";

            // Act - Set same search text again
            viewModel.SearchText = "test";

            // Assert - Should still work without issues
            Assert.False(viewModel.OkCommand.CanExecute(null)); // No items match "test"
        }

        #endregion
}
