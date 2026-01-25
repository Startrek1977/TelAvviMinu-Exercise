using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.ViewModels
{
    /// <summary>
    /// Partial class containing dialog-related properties and commands for the DataBrowserDialog.
    /// Implements IDeferredInitialization for View-First pattern and IColumnConfiguration for custom columns.
    /// </summary>
    public partial class MainWindowViewModel : IDeferredInitialization, IColumnConfiguration
    {
        private ICollectionView? _dialogFilteredItems;
        private ObservableCollection<BrowserColumn>? _dialogColumns;
        private IEnumerable? _dialogItemsSource;

        // Deferred initialization data
        private IEnumerable? _pendingDialogItems;
        private object? _pendingDialogSelection;

        private bool _dialogIsLoading = true;
        /// <summary>
        /// Indicates whether the dialog is still loading data.
        /// </summary>
        public bool DialogIsLoading
        {
            get => _dialogIsLoading;
            private set => SetProperty(ref _dialogIsLoading, value);
        }

        private string _dialogSearchText = string.Empty;
        /// <summary>
        /// Gets or sets the search text used to filter the items collection in the dialog.
        /// </summary>
        public string DialogSearchText
        {
            get => _dialogSearchText;
            set
            {
                if (SetProperty(ref _dialogSearchText, value))
                {
                    RefreshDialogFilterWithSelectionPreservation();
                }
            }
        }

        /// <summary>
        /// Indicates whether the dialog has search text.
        /// </summary>
        public bool DialogHasSearchText => !string.IsNullOrWhiteSpace(DialogSearchText);

        /// <summary>
        /// Gets the custom column configurations for the dialog's DataGrid.
        /// </summary>
        public ObservableCollection<BrowserColumn>? Columns => _dialogColumns;

        /// <summary>
        /// Indicates whether custom columns are defined for the dialog.
        /// </summary>
        public bool HasCustomColumns => _dialogColumns != null && _dialogColumns.Count > 0;

        private object? _dialogSelectedItem;
        /// <summary>
        /// Gets or sets the currently selected item in the dialog.
        /// </summary>
        public object? DialogSelectedItem
        {
            get => _dialogSelectedItem;
            set
            {
                if (SetProperty(ref _dialogSelectedItem, value))
                {
                    DialogOkCommand.NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets the filtered items collection for the dialog's DataGrid.
        /// </summary>
        public ICollectionView? DialogFilteredItems => _dialogFilteredItems;

        /// <summary>
        /// Gets the count of items in the filtered collection.
        /// </summary>
        public int DialogItemsCount => _dialogFilteredItems?.Cast<object>().Count() ?? 0;

        private bool? _dialogResult;
        /// <summary>
        /// Gets or sets the dialog result (true for OK, false for Cancel).
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        private ICommand? _dialogOkCommand;
        /// <summary>
        /// Command to confirm the dialog selection.
        /// </summary>
        public ICommand DialogOkCommand => _dialogOkCommand ??= new RelayCommand(OnDialogOk, CanDialogOk);

        private ICommand? _dialogCancelCommand;
        /// <summary>
        /// Command to cancel the dialog.
        /// </summary>
        public ICommand DialogCancelCommand => _dialogCancelCommand ??= new RelayCommand(OnDialogCancel);

        private ICommand? _dialogClearSearchCommand;
        /// <summary>
        /// Command to clear the search text in the dialog.
        /// </summary>
        public ICommand DialogClearSearchCommand => _dialogClearSearchCommand ??= new RelayCommand(OnDialogClearSearch);

        /// <summary>
        /// Prepares the dialog state for showing a browse dialog.
        /// Call this before showing the DataBrowserDialog.
        /// </summary>
        /// <param name="items">The items to browse.</param>
        /// <param name="currentSelection">The currently selected item.</param>
        /// <param name="columns">Optional custom column configurations.</param>
        public void PrepareDialog(IEnumerable? items, object? currentSelection, ObservableCollection<BrowserColumn>? columns = null)
        {
            // Store for deferred initialization
            _pendingDialogItems = items;
            _pendingDialogSelection = currentSelection;
            _dialogColumns = columns;

            // Reset dialog state
            _dialogSearchText = string.Empty;
            _dialogSelectedItem = null;
            _dialogResult = null;
            _dialogIsLoading = true;

            // Create a new ObservableCollection for the dialog items
            _dialogItemsSource = new ObservableCollection<object>();
            _dialogFilteredItems = CollectionViewSource.GetDefaultView(_dialogItemsSource);
            _dialogFilteredItems.Filter = DialogFilterItems;

            // Notify property changes
            OnPropertyChanged(nameof(DialogSearchText));
            OnPropertyChanged(nameof(DialogHasSearchText));
            OnPropertyChanged(nameof(DialogSelectedItem));
            OnPropertyChanged(nameof(DialogResult));
            OnPropertyChanged(nameof(DialogFilteredItems));
            OnPropertyChanged(nameof(DialogItemsCount));
            OnPropertyChanged(nameof(DialogIsLoading));
            OnPropertyChanged(nameof(Columns));
            OnPropertyChanged(nameof(HasCustomColumns));
        }

        /// <summary>
        /// Initializes the dialog by loading the data.
        /// Called from the dialog's ContentRendered event via View-First pattern.
        /// </summary>
        public void Initialize()
        {
            if (_pendingDialogItems != null && _dialogItemsSource is ObservableCollection<object> items)
            {
                foreach (var item in _pendingDialogItems)
                {
                    items.Add(item);
                }
                _pendingDialogItems = null;
            }

            _dialogFilteredItems?.Refresh();
            OnPropertyChanged(nameof(DialogItemsCount));

            DialogSelectedItem = FindMatchingDialogItem(_pendingDialogSelection);
            _pendingDialogSelection = null;

            DialogIsLoading = false;
        }

        /// <summary>
        /// Refreshes the filtered items collection while preserving the current selection.
        /// </summary>
        private void RefreshDialogFilterWithSelectionPreservation()
        {
            _dialogFilteredItems?.Refresh();
            OnPropertyChanged(nameof(DialogItemsCount));
            OnPropertyChanged(nameof(DialogHasSearchText));

            // Re-notify SelectedItem to update DataGrid selection after filter changes
            if (_dialogSelectedItem != null && _dialogFilteredItems?.Cast<object>().Contains(_dialogSelectedItem) == true)
            {
                OnPropertyChanged(nameof(DialogSelectedItem));
            }

            // Notify the OK command to re-evaluate its CanExecute state
            DialogOkCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Filter predicate for the dialog's items collection.
        /// </summary>
        private bool DialogFilterItems(object item)
        {
            if (string.IsNullOrWhiteSpace(DialogSearchText))
                return true;

            var searchLower = DialogSearchText.ToLower();
            var type = item.GetType();

            foreach (var property in type.GetProperties())
            {
                var value = property.GetValue(item);
                if (value != null && value.ToString()?.ToLower().Contains(searchLower) == true)
                    return true;
            }

            return false;
        }

        private void OnDialogOk()
        {
            DialogResult = true;
        }

        private bool CanDialogOk()
        {
            if (DialogSelectedItem == null)
                return false;

            return _dialogFilteredItems?.Cast<object>().Contains(DialogSelectedItem) == true;
        }

        private void OnDialogCancel()
        {
            DialogResult = false;
        }

        private void OnDialogClearSearch()
        {
            DialogSearchText = string.Empty;
        }

        /// <summary>
        /// Finds a matching item in the dialog items collection for the given selection.
        /// Uses IEntity.Id for entities, then Equals, then ReferenceEquals as fallback.
        /// </summary>
        private object? FindMatchingDialogItem(object? selection)
        {
            if (selection == null || _dialogItemsSource is not ObservableCollection<object> items)
                return null;

            // For IEntity types, match by Id
            if (selection is IEntity selectedEntity)
            {
                return items.FirstOrDefault(item =>
                    item is IEntity entity && entity.Id == selectedEntity.Id);
            }

            // Try Equals comparison
            var equalMatch = items.FirstOrDefault(item => item.Equals(selection));
            if (equalMatch != null)
                return equalMatch;

            // Fall back to reference equality
            return items.FirstOrDefault(item => ReferenceEquals(item, selection));
        }
    }
}
