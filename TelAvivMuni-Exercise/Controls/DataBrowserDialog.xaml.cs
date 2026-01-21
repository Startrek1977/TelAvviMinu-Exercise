using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TelAvivMuni_Exercise.Infrastructure;

namespace TelAvivMuni_Exercise.Controls
{
    /// <summary>
    /// Dialog window for browsing and selecting items from a collection with filtering support.
    /// Uses View-First initialization: the dialog is created and rendered first, then
    /// data is loaded via ViewModel.Initialize() in the ContentRendered event.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class DataBrowserDialog : Window
    {
        public DataBrowserDialog()
        {
            InitializeComponent();
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
            if (DataContext is not IColumnConfiguration columnConfig || !columnConfig.HasCustomColumns)
                return;

            // Find the custom column configuration for this property
            var customColumn = columnConfig.Columns?.FirstOrDefault(c => c.DataField == e.PropertyName);

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
