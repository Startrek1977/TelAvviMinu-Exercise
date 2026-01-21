using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace TelAvivMuni_Exercise.Infrastructure.Behaviors
{
    /// <summary>
    /// Attached behavior that scrolls a DataGrid to bring the selected item into view.
    /// When the bound SelectedItem changes, the DataGrid automatically scrolls to show it.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DataGridScrollIntoViewBehavior
    {
        /// <summary>
        /// The item to scroll into view. Bind this to the ViewModel's SelectedItem property.
        /// </summary>
        public static readonly DependencyProperty ScrollIntoViewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollIntoViewItem",
                typeof(object),
                typeof(DataGridScrollIntoViewBehavior),
                new PropertyMetadata(null, OnScrollIntoViewItemChanged));

        public static object GetScrollIntoViewItem(DependencyObject obj) =>
            obj.GetValue(ScrollIntoViewItemProperty);

        public static void SetScrollIntoViewItem(DependencyObject obj, object value) =>
            obj.SetValue(ScrollIntoViewItemProperty, value);

        private static void OnScrollIntoViewItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && e.NewValue != null)
            {
                dataGrid.ScrollIntoView(e.NewValue);
            }
        }
    }
}
