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

                    // Re-synchronize the ICollectionView's CurrentItem
                    viewModel.FilteredItems.MoveCurrentTo(selectedItem);

                    // Force the DataGrid to update its selection by clearing and re-setting
                    ProductsDataGrid.SelectedItem = null;
                    ProductsDataGrid.UpdateLayout();
                    ProductsDataGrid.SelectedItem = selectedItem;

                    // Scroll into view and focus
                    ProductsDataGrid.ScrollIntoView(selectedItem);
                    ProductsDataGrid.Focus();
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
