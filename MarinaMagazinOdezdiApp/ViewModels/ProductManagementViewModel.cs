using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input; // Added for ICommand
using MarinaMagazinOdezdiApp.Views;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class ProductManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Category> _categories;

        public ObservableCollection<Product> Products { get; set; }

        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductManagementViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Products = new ObservableCollection<Product>();
            _categories = new ObservableCollection<Category>();

            AddProductCommand = new AsyncRelayCommand(AddProduct);
            EditProductCommand = new AsyncRelayCommand<Product>(EditProduct);
            DeleteProductCommand = new AsyncRelayCommand<Product>(DeleteProduct);
        }

        public async Task LoadDataAsync()
        {
            var products = await _databaseService.GetProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            var categories = await _databaseService.GetCategoriesAsync();
            _categories.Clear();
            foreach (var category in categories)
            {
                _categories.Add(category);
            }
        }

        private async Task AddProduct()
        {
            var newProduct = new Product();
            var viewModel = new AddEditProductViewModel(newProduct, _categories);
            var window = new AddEditProductWindow { DataContext = viewModel };

            if (window.ShowDialog() == true)
            {
                await _databaseService.AddProductAsync(viewModel.Product);
                await LoadDataAsync();
            }
        }

        private async Task EditProduct(Product product)
        {
            if (product == null) return;

            // Create a copy for editing
            var productCopy = new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                StockQuantity = product.StockQuantity
            };

            var viewModel = new AddEditProductViewModel(productCopy, _categories);
            var window = new AddEditProductWindow { DataContext = viewModel };

            if (window.ShowDialog() == true)
            {
                await _databaseService.UpdateProductAsync(viewModel.Product);
                await LoadDataAsync();
            }
        }

        private async Task DeleteProduct(Product product)
        {
            if (product == null) return;

            if (MessageBox.Show($"Вы уверены, что хотите удалить товар '{product.Name}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _databaseService.DeleteProductAsync(product.ProductId);
                    await LoadDataAsync();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
