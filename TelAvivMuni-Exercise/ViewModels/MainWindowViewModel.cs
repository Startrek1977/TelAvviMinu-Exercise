using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private ObservableCollection<Product> _products = new();
        private string? _errorMessage;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
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

        /// <summary>
        /// Runtime constructor used by XAML. Requires WPF Application context.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MainWindowViewModel() : this(App.UnitOfWork)
        {
        }

        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load products: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task<OperationResult> AddProductAsync(Product product)
        {
            ErrorMessage = null;
            var result = await _unitOfWork.Products.AddAsync(product);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return result;
            }
            await _unitOfWork.SaveChangesAsync();
            Products.Add(product);
            return result;
        }

        [RelayCommand]
        private async Task<OperationResult> UpdateProductAsync(Product product)
        {
            ErrorMessage = null;
            var result = await _unitOfWork.Products.UpdateAsync(product);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return result;
            }
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        [RelayCommand]
        private async Task<OperationResult> DeleteProductAsync(Product product)
        {
            ErrorMessage = null;
            var result = await _unitOfWork.Products.DeleteAsync(product);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return result;
            }
            await _unitOfWork.SaveChangesAsync();
            Products.Remove(product);
            return result;
        }

        [RelayCommand]
        private async Task<OperationResult> SaveChangesAsync()
        {
            ErrorMessage = null;
            try
            {
                await _unitOfWork.SaveChangesAsync();
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save changes: {ex.Message}";
                return OperationResult.Fail(ErrorMessage);
            }
        }
    }
}
