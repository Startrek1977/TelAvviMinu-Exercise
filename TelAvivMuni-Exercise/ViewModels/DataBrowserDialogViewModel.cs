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
    public class DataBrowserDialogViewModel : ObservableObject
    {
        private readonly ObservableCollection<object> _items;
        private readonly ICollectionView _filteredItems;
        private readonly ObservableCollection<BrowserColumn>? _columns;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _filteredItems.Refresh();
                    OnPropertyChanged(nameof(ItemsCount));
                    OnPropertyChanged(nameof(HasSearchText));
                }
            }
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

            // Set the selected item - it should match by reference with items in the collection
            if (currentSelection != null)
            {
                // Find the matching item by reference
                var matchedItem = _items.FirstOrDefault(item => ReferenceEquals(item, currentSelection));
                if (matchedItem != null)
                {
                    SelectedItem = matchedItem;
                    // Also move the current item in the collection view
                    _filteredItems.MoveCurrentTo(matchedItem);
                }
            }
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

        private bool CanOk()
        {
            return SelectedItem != null;
        }

        private void OnCancel()
        {
            DialogResult = false;
        }

        private void OnClearSearch()
        {
            SearchText = string.Empty;
        }
    }
}
