using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TelAvivMuni_Exercise.ViewModels;

namespace TelAvivMuni_Exercise.Controls
{
    public partial class DataBrowserDialog : Window
    {
        private bool _isUpdatingSelection = false;

        public DataBrowserDialog()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure the selected item is scrolled into view when the dialog opens
            if (DataContext is DataBrowserDialogViewModel viewModel && viewModel.SelectedItem != null)
            {
                // Capture the selected item before the async operation
                var selectedItem = viewModel.SelectedItem;

                // Use ContextIdle priority to ensure DataGrid has fully rendered and bound
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Verify the selected item is still valid
                    if (selectedItem == null)
                        return;

                    _isUpdatingSelection = true;
                    try
                    {
                        // Re-synchronize the ICollectionView's CurrentItem
                        viewModel.FilteredItems.MoveCurrentTo(selectedItem);

                        // Force the DataGrid to update its selection by clearing and re-setting
                        ProductsDataGrid.SelectedItem = null;
                        ProductsDataGrid.UpdateLayout();
                        ProductsDataGrid.SelectedItem = selectedItem;

                        // Scroll into view and focus
                        ProductsDataGrid.ScrollIntoView(selectedItem);
                        ProductsDataGrid.Focus();
                    }
                    finally
                    {
                        _isUpdatingSelection = false;
                    }
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DataBrowserDialogViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (e.NewValue is DataBrowserDialogViewModel newViewModel)
            {
                newViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataBrowserDialogViewModel.DialogResult))
            {
                if (DataContext is DataBrowserDialogViewModel viewModel && viewModel.DialogResult.HasValue)
                {
                    DialogResult = viewModel.DialogResult;
                }
            }
            else if (e.PropertyName == nameof(DataBrowserDialogViewModel.SearchText))
            {
                // When search text changes, preserve the DataGrid selection
                if (DataContext is DataBrowserDialogViewModel viewModel && viewModel.SelectedItem != null)
                {
                    var selectedItem = viewModel.SelectedItem;

                    // Use a dispatcher to ensure the filtered items have been updated
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Check if the selected item is in the filtered collection
                        bool isInFilteredCollection = viewModel.FilteredItems.Cast<object>().Contains(selectedItem);

                        if (isInFilteredCollection)
                        {
                            _isUpdatingSelection = true;
                            try
                            {
                                // Item is visible, force selection refresh
                                ProductsDataGrid.SelectedItem = null;
                                ProductsDataGrid.UpdateLayout();
                                ProductsDataGrid.SelectedItem = selectedItem;

                                // Scroll into view if needed
                                try
                                {
                                    ProductsDataGrid.ScrollIntoView(selectedItem);
                                }
                                catch
                                {
                                    // Ignore scroll errors
                                }
                            }
                            finally
                            {
                                _isUpdatingSelection = false;
                            }
                        }
                    }), System.Windows.Threading.DispatcherPriority.ContextIdle);
                }
            }
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't update ViewModel if we're programmatically updating the selection
            if (_isUpdatingSelection)
                return;

            // Update the ViewModel when user manually selects an item
            if (DataContext is DataBrowserDialogViewModel viewModel && e.AddedItems.Count > 0)
            {
                viewModel.SelectedItem = e.AddedItems[0];
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (DataContext is not DataBrowserDialogViewModel viewModel || !viewModel.HasCustomColumns)
                return;

            var customColumn = viewModel.Columns?.FirstOrDefault(c => c.DataField == e.PropertyName);

            if (customColumn == null)
            {
                e.Cancel = true; // Hide columns not in custom configuration
                return;
            }

            // Set custom header
            e.Column.Header = customColumn.Header;

            // Set custom width
            if (!double.IsNaN(customColumn.Width))
            {
                e.Column.Width = new DataGridLength(customColumn.Width);
            }

            // Apply formatting and alignment for text columns
            if (e.Column is DataGridTextColumn textColumn)
            {
                // Apply format if specified
                if (!string.IsNullOrEmpty(customColumn.Format))
                {
                    textColumn.Binding = new Binding(e.PropertyName)
                    {
                        StringFormat = customColumn.Format
                    };
                }

                // Apply alignment if specified
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
