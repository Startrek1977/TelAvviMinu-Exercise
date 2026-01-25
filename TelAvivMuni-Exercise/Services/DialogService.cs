using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using TelAvivMuni_Exercise.Controls;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.ViewModels;

namespace TelAvivMuni_Exercise.Services
{
    [ExcludeFromCodeCoverage]
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

            // Use MainWindowViewModel directly - no separate dialog ViewModel needed
            var mainViewModel = mainWindow.DataContext as MainWindowViewModel;
            if (mainViewModel == null)
            {
                MessageBox.Show("Unable to access main view model.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return currentSelection;
            }

            // Prepare the dialog state on the main ViewModel
            mainViewModel.PrepareDialog(items, currentSelection, columns);

            var dialog = new DataBrowserDialog
            {
                DataContext = mainViewModel,
                Title = title,
                Owner = mainWindow
            };

            // Position dialog to the right of the main window
            dialog.Left = mainWindow.Left + mainWindow.ActualWidth;
            dialog.Top = mainWindow.Top;

            if (dialog.ShowDialog() == true)
            {
                return mainViewModel.DialogSelectedItem;
            }

            return currentSelection;
        }
    }
}
