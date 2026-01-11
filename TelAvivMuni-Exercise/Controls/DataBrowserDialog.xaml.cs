using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TelAvivMuni_Exercise.ViewModels;

namespace TelAvivMuni_Exercise.Controls
{
    /// <summary>
    /// Dialog window for browsing and selecting items from a collection with filtering support.
    /// Uses attached behaviors for keyboard handling and dialog closing (MVVM pattern).
    /// </summary>
    public partial class DataBrowserDialog : Window
    {
        /// <summary>
        /// Flag to prevent circular updates between DataGrid and ViewModel when programmatically updating selection.
        /// </summary>
        private bool _isUpdatingSelection = false;

        public DataBrowserDialog()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        /// <summary>
        /// Handles the Closed event to clean up event subscriptions and prevent memory leaks.
        /// This ensures the window can be properly garbage collected.
        /// </summary>
        private void OnClosed(object? sender, EventArgs e)
        {
            // Unsubscribe from all events to prevent memory leaks
            DataContextChanged -= OnDataContextChanged;
            Loaded -= OnLoaded;
            Closed -= OnClosed;

            // Unsubscribe from ViewModel events
            if (DataContext is DataBrowserDialogViewModel viewModel)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        /// <summary>
        /// Synchronizes the DataGrid's selected item with the ViewModel's SelectedItem property.
        /// This method uses a workaround to force the DataGrid to update its selection by temporarily
        /// clearing it before setting it again, which ensures the item is properly highlighted and scrolled into view.
        /// </summary>
        /// <param name="selectedItem">The item to select in the DataGrid</param>
        private void SynchronizeDataGridSelection(object selectedItem)
        {
            if (selectedItem == null)
                return;

            // Set flag to prevent SelectionChanged event from updating the ViewModel
            _isUpdatingSelection = true;
            try
            {
                // Update the collection view's current item
                if (DataContext is DataBrowserDialogViewModel viewModel)
                {
                    viewModel.FilteredItems.MoveCurrentTo(selectedItem);
                }

                // Force DataGrid to refresh selection by clearing and resetting
                // This workaround ensures proper highlighting and scrolling
                ProductsDataGrid.SelectedItem = null;
                ProductsDataGrid.UpdateLayout();
                ProductsDataGrid.SelectedItem = selectedItem;
                ProductsDataGrid.ScrollIntoView(selectedItem);
            }
            finally
            {
                // Always restore flag to allow future user selections
                _isUpdatingSelection = false;
            }
        }

        /// <summary>
        /// Handles the Loaded event to ensure the initially selected item is properly displayed when the dialog opens.
        /// Uses Dispatcher.BeginInvoke with ContextIdle priority to ensure DataGrid is fully rendered before selection.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure the selected item is scrolled into view when the dialog opens
            if (DataContext is DataBrowserDialogViewModel viewModel && viewModel.SelectedItem != null)
            {
                var selectedItem = viewModel.SelectedItem;

                // Dispatcher is needed here because the Loaded event fires before the DataGrid
                // has fully rendered and virtualized its items. Without ContextIdle priority,
                // ScrollIntoView may fail because the visual tree is not yet complete.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SynchronizeDataGridSelection(selectedItem);
                    ProductsDataGrid.Focus();
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        /// <summary>
        /// Handles DataContext changes to subscribe/unsubscribe from ViewModel PropertyChanged events.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from the old ViewModel's PropertyChanged event to prevent memory leaks
            if (e.OldValue is DataBrowserDialogViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            // Subscribe to the new ViewModel's PropertyChanged event
            if (e.NewValue is DataBrowserDialogViewModel newViewModel)
            {
                newViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        /// <summary>
        /// Handles ViewModel property changes to synchronize UI state with the ViewModel.
        /// Specifically handles SearchText changes to preserve selection when filtering.
        /// DialogResult handling is now done via DialogCloseBehavior.
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Handle SearchText changes to preserve selection when filtering
            if (e.PropertyName == nameof(DataBrowserDialogViewModel.SearchText))
            {
                // When search text changes, preserve the DataGrid selection if it's still visible
                if (DataContext is DataBrowserDialogViewModel viewModel && viewModel.SelectedItem != null)
                {
                    var selectedItem = viewModel.SelectedItem;

                    // Check if the selected item is still visible in the filtered collection
                    bool isInFilteredCollection = viewModel.FilteredItems.Cast<object>().Contains(selectedItem);

                    if (isInFilteredCollection)
                    {
                        try
                        {
                            SynchronizeDataGridSelection(selectedItem);
                        }
                        catch
                        {
                            // Ignore scroll errors if item is not yet in the visual tree
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles DataGrid SelectionChanged events to update the ViewModel when the user manually selects an item.
        /// Ignores selection changes triggered programmatically by SynchronizeDataGridSelection.
        /// </summary>
        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't update ViewModel if we're programmatically updating the selection
            // to prevent circular updates
            if (_isUpdatingSelection)
                return;

            // Update the ViewModel when user manually selects an item from the DataGrid
            if (DataContext is DataBrowserDialogViewModel viewModel && e.AddedItems.Count > 0)
            {
                viewModel.SelectedItem = e.AddedItems[0];
            }
        }

        /// <summary>
        /// Handles the AutoGeneratingColumn event to apply custom column configurations.
        /// This event is fired for each column as the DataGrid auto-generates columns from the data source.
        /// Custom columns can specify header text, width, format, and horizontal alignment.
        /// Columns not in the custom configuration are hidden.
        /// </summary>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Only apply custom column configuration if columns are defined in the ViewModel
            if (DataContext is not DataBrowserDialogViewModel viewModel || !viewModel.HasCustomColumns)
                return;

            // Find the custom column configuration for this property
            var customColumn = viewModel.Columns?.FirstOrDefault(c => c.DataField == e.PropertyName);

            if (customColumn == null)
            {
                // Hide columns that are not in the custom configuration
                e.Cancel = true;
                return;
            }

            // Set custom header text
            e.Column.Header = customColumn.Header;

            // Set custom width if specified
            if (!double.IsNaN(customColumn.Width))
            {
                e.Column.Width = new DataGridLength(customColumn.Width);
            }

            // Apply formatting and alignment for text columns
            if (e.Column is DataGridTextColumn textColumn)
            {
                // Apply format string if specified (e.g., currency, date formats)
                if (!string.IsNullOrEmpty(customColumn.Format))
                {
                    textColumn.Binding = new Binding(e.PropertyName)
                    {
                        StringFormat = customColumn.Format
                    };
                }

                // Apply horizontal alignment if specified
                if (!string.IsNullOrEmpty(customColumn.HorizontalAlignment))
                {
                    var style = new Style(typeof(TextBlock));
                    if (customColumn.HorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
                    {
                        style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                    }
                    else if (customColumn.HorizontalAlignment.Equals("Center", StringComparison.OrdinalIgnoreCase))
                    {
                        style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                    }
                    textColumn.ElementStyle = style;
                }
            }
        }
    }
}
