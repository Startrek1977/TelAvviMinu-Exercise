using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.ViewModels
{
    /// <summary>
    /// ViewModel for the DataBrowserDialog which provides filtering and selection functionality
    /// for browsing a collection of items.
    /// Uses View-First initialization pattern: the View is created first, then data is loaded
    /// after the View is fully rendered via InitializeAsync().
    /// </summary>
    public class DataBrowserDialogViewModel : ObservableObject, IDeferredInitialization, IColumnConfiguration
    {
        private readonly ObservableCollection<object> _items;
        private readonly ICollectionView _filteredItems;
        private readonly ObservableCollection<BrowserColumn>? _columns;

        // Deferred initialization data
        private IEnumerable? _pendingItems;
        private object? _pendingSelection;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Gets or sets the search text used to filter the items collection.
        /// </summary>
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    RefreshFilterWithSelectionPreservation();
                }
            }
        }

        /// <summary>
        /// Refreshes the filtered items collection while preserving the current selection.
        /// This method is called when the search text changes.
        /// </summary>
        private void RefreshFilterWithSelectionPreservation()
        {
            // Refresh the filtered collection based on the current search text
            _filteredItems.Refresh();
            OnPropertyChanged(nameof(ItemsCount));
            OnPropertyChanged(nameof(HasSearchText));

            // Re-notify SelectedItem to update DataGrid selection after filter changes
            if (_selectedItem != null && _filteredItems.Cast<object>().Contains(_selectedItem))
            {
                    // Explicitly notify that SelectedItem hasn't changed to force DataGrid update
                    // This is especially important when clearing the filter
                OnPropertyChanged(nameof(SelectedItem));
            }

            // Notify the OK command to re-evaluate its CanExecute state
            // This will disable the OK button if the selected item is filtered out
            OkCommand.NotifyCanExecuteChanged();
        }

        public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

        public ObservableCollection<BrowserColumn>? Columns => _columns;

        public bool HasCustomColumns => _columns != null && _columns.Count > 0;

        private object? _selectedItem;
        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    OkCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ICollectionView FilteredItems => _filteredItems;

        public int ItemsCount => _filteredItems.Cast<object>().Count();

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        private ICommand? _okCommand;
        public ICommand OkCommand => _okCommand ??= new RelayCommand(OnOk, CanOk);

        private ICommand? _cancelCommand;
        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(OnCancel);

        private ICommand? _clearSearchCommand;
        public ICommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand(OnClearSearch);

        /// <summary>
        /// Creates a ViewModel with deferred data loading.
        /// Call InitializeAsync() after the View is fully rendered to load the data.
        /// </summary>
        public DataBrowserDialogViewModel(IEnumerable? items, object? currentSelection, ObservableCollection<BrowserColumn>? columns = null)
        {
            _items = new ObservableCollection<object>();
            _columns = columns;
            _pendingItems = items;
            _pendingSelection = currentSelection;

            _filteredItems = CollectionViewSource.GetDefaultView(_items);
            _filteredItems.Filter = FilterItems;
        }

        /// <summary>
        /// Initializes the ViewModel by loading the data.
        /// Call this after the View is fully rendered (e.g., in ContentRendered event).
        /// </summary>
        public void Initialize()
        {
            if (_pendingItems != null)
            {
                foreach (var item in _pendingItems)
                {
                    _items.Add(item);
                }
                _pendingItems = null;
            }

            _filteredItems.Refresh();
            OnPropertyChanged(nameof(ItemsCount));

            SelectedItem = FindMatchingItem(_pendingSelection);
            _pendingSelection = null;

            IsLoading = false;
        }

        private bool FilterItems(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            var searchLower = SearchText.ToLower();
            var type = item.GetType();

            foreach (var property in type.GetProperties())
            {
                var value = property.GetValue(item);
                if (value != null && value.ToString()?.ToLower().Contains(searchLower) == true)
                    return true;
            }

            return false;
        }

        private void OnOk()
        {
            DialogResult = true;
        }

        /// <summary>
        /// Determines whether the OK button should be enabled.
        /// The button is enabled only if an item is selected AND it's visible in the filtered collection.
        /// </summary>
        private bool CanOk()
        {
            // Check if an item is selected
            if (SelectedItem == null)
                return false;

            // Check if the selected item is visible in the filtered collection
            // (not filtered out by search text)
            return _filteredItems.Cast<object>().Contains(SelectedItem);
        }

        private void OnCancel()
        {
            DialogResult = false;
        }

        private void OnClearSearch()
        {
            SearchText = string.Empty;
        }

        /// <summary>
        /// Finds a matching item in the items collection for the given selection.
        /// Uses IEntity.Id for entities, then Equals, then ReferenceEquals as fallback.
        /// </summary>
        private object? FindMatchingItem(object? selection)
        {
            if (selection == null)
                return null;

            // For IEntity types, match by Id
            if (selection is IEntity selectedEntity)
            {
                return _items.FirstOrDefault(item =>
                    item is IEntity entity && entity.Id == selectedEntity.Id);
            }

            // Try Equals comparison
            var equalMatch = _items.FirstOrDefault(item => item.Equals(selection));
            if (equalMatch != null)
                return equalMatch;

            // Fall back to reference equality
            return _items.FirstOrDefault(item => ReferenceEquals(item, selection));
        }
    }
}
