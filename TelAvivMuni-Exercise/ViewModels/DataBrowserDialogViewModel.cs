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
    /// </summary>
    public class DataBrowserDialogViewModel : ObservableObject
    {
        private readonly ObservableCollection<object> _items;
        private readonly ICollectionView _filteredItems;
        private ObservableCollection<BrowserColumn>? _columns;

        // Stores the pending selection to be applied after the view is fully loaded
        private object? _pendingSelection;

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
            // Preserve the selected item before refreshing the filter
            var currentSelection = _selectedItem;

            // Refresh the filtered collection based on the current search text
            _filteredItems.Refresh();
            OnPropertyChanged(nameof(ItemsCount));
            OnPropertyChanged(nameof(HasSearchText));

            // Always restore the selection to prevent it from being cleared
            if (currentSelection != null)
            {
                // Check if the selected item is still visible after filtering
                if (_filteredItems.Cast<object>().Contains(currentSelection))
                {
                    // Item is still visible, move the collection view to it
                    _filteredItems.MoveCurrentTo(currentSelection);

                    // Explicitly notify that SelectedItem hasn't changed to force DataGrid update
                    // This is especially important when clearing the filter
                    OnPropertyChanged(nameof(SelectedItem));
                }
                // Don't clear the SelectedItem even if it's filtered out
                // This preserves the selection in the ViewModel
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

        public DataBrowserDialogViewModel(IEnumerable items, object? currentSelection, ObservableCollection<BrowserColumn>? columns = null)
        {
            _items = new ObservableCollection<object>();
            _columns = columns;

            if (items != null)
            {
                foreach (var item in items)
                {
                    _items.Add(item);
                }
            }

            _filteredItems = CollectionViewSource.GetDefaultView(_items);
            _filteredItems.Filter = FilterItems;

            // Store the pending selection to be applied after the view is fully rendered
            _pendingSelection = currentSelection;
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
        /// Applies the pending selection after the view is fully loaded and rendered.
        /// This ensures the DataGrid properly reflects the selected item with highlighting.
        /// Should be called from the view's ContentRendered event.
        /// </summary>
        public void ApplyPendingSelection()
        {
            if (_pendingSelection != null)
            {
                // Find the matching item by reference in the current items collection
                var matchedItem = _items.FirstOrDefault(item => ReferenceEquals(item, _pendingSelection));
                if (matchedItem != null)
                {
                    SelectedItem = matchedItem;
                    _filteredItems.MoveCurrentTo(matchedItem);
                }
                _pendingSelection = null;
            }
        }
    }
}
