using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<Product> _products = new();
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private Product? _selectedProduct1;
        public Product? SelectedProduct1
        {
            get => _selectedProduct1;
            set => SetProperty(ref _selectedProduct1, value);
        }

        private Product? _selectedProduct2;
        public Product? SelectedProduct2
        {
            get => _selectedProduct2;
            set => SetProperty(ref _selectedProduct2, value);
        }
    }
}
