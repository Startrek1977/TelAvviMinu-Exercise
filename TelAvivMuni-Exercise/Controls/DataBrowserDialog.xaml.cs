using System.Collections;
using System.Windows;

namespace TelAvivMuni_Exercise.Controls
{
    public partial class DataBrowserDialog : Window
    {
        public object? SelectedItem { get; private set; }

        public DataBrowserDialog()
        {
            InitializeComponent();
        }

        public void SetItemsSource(IEnumerable? items)
        {
            ItemsDataGrid.ItemsSource = items;
            UpdateItemsCount();
        }

        private void UpdateItemsCount()
        {
            var count = 0;
            if (ItemsDataGrid.ItemsSource != null)
            {
                foreach (var _ in ItemsDataGrid.ItemsSource)
                {
                    count++;
                }
            }
            ItemsCountTextBlock.Text = $"{count} items";
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            SelectedItem = ItemsDataGrid.SelectedItem;
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
