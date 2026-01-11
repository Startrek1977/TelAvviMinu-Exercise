using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using TelAvivMuni_Exercise.Controls;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.ViewModels;

namespace TelAvivMuni_Exercise.Services
{
    public class DialogService : IDialogService
    {
        public object? ShowBrowseDialog(IEnumerable items, string title, object? currentSelection, ObservableCollection<BrowserColumn>? columns = null)
        {
            if (items == null)
            {
                MessageBox.Show("No items available to display.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return currentSelection;
            }

            var mainWindow = Application.Current.MainWindow;

            // Create fresh ViewModel and Dialog each time
            // The ViewModel is bound to the dialog via DialogCloseBehavior which requires a fresh instance
            var viewModel = new DataBrowserDialogViewModel();
            viewModel.Reset(items, currentSelection, columns);

            var dialog = new DataBrowserDialog
            {
                DataContext = viewModel,
                Title = title,
                Owner = mainWindow
            };

            // Position dialog to the right of the main window
            dialog.Left = mainWindow.Left + mainWindow.ActualWidth;
            dialog.Top = mainWindow.Top;

            if (dialog.ShowDialog() == true)
            {
                return viewModel.SelectedItem;
            }

            return currentSelection;
        }
    }
}
