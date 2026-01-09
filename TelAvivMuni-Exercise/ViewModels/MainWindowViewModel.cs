using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
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

        public MainWindowViewModel()
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Products.json");
                if (!File.Exists(jsonPath))
                {
                    Products = new ObservableCollection<Product>();
                    return;
                }

                var json = File.ReadAllText(jsonPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Products = new ObservableCollection<Product>();
                    return;
                }

                var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Products = products != null && products.Count > 0
                    ? new ObservableCollection<Product>(products)
                    : new ObservableCollection<Product>();
            }
            catch (JsonException)
            {
                Products = new ObservableCollection<Product>();
            }
            catch (IOException)
            {
                Products = new ObservableCollection<Product>();
            }
            catch (UnauthorizedAccessException)
            {
                Products = new ObservableCollection<Product>();
            }
        }
    }
}
