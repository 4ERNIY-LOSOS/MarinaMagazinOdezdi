using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MarinaMagazinOdezdiApp.Commands; // Use the new centralized command classes

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class ProductsViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationViewModel _appViewModel;
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;

        private List<Product> _allProducts; // Master list of all products

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Category> Categories { get; set; }

        public List<string> SortOptions { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set 
            {
                SetProperty(ref _searchText, value);
                FilterAndSortProducts();
            }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                FilterAndSortProducts();
            }
        }

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                SetProperty(ref _selectedSortOption, value);
                FilterAndSortProducts();
            }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand ViewCartCommand { get; }
        public ICommand ViewProfileCommand { get; }

        public ProductsViewModel(ApplicationViewModel appViewModel, DatabaseService databaseService, User currentUser)
        {
            _appViewModel = appViewModel;
            _databaseService = databaseService;
            _currentUser = currentUser;
            
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<Category>();
            SortOptions = new List<string> { "По умолчанию", "Сначала дешевые", "Сначала дорогие" };
            _selectedSortOption = SortOptions.First();

            AddToCartCommand = new AsyncRelayCommand<Product>(AddToCart);
            ViewCartCommand = new RelayCommand(ViewCart);
            ViewProfileCommand = new RelayCommand(ViewProfile);
        }

        public async Task InitializeAsync()
        {
            await LoadCategories();
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            _allProducts = await _databaseService.GetProductsAsync();
            FilterAndSortProducts(); // Initial load
        }

        private async Task LoadCategories()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            Categories.Clear();
            Categories.Add(new Category { CategoryId = 0, CategoryName = "Все категории" }); // Add "All" option
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            _selectedCategory = Categories.First();
        }

        private void FilterAndSortProducts()
        {
            IEnumerable<Product> filteredProducts = _allProducts;

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredProducts = filteredProducts.Where(p => p.Name.ToLower().Contains(SearchText.ToLower()));
            }

            // Category filter
            if (SelectedCategory != null && SelectedCategory.CategoryId != 0) // 0 is "All categories"
            {
                filteredProducts = filteredProducts.Where(p => p.CategoryId == SelectedCategory.CategoryId);
            }

            // Sorting
            if (SelectedSortOption == "Сначала дешевые")
            {
                filteredProducts = filteredProducts.OrderBy(p => p.Price);
            }
            else if (SelectedSortOption == "Сначала дорогие")
            {
                filteredProducts = filteredProducts.OrderByDescending(p => p.Price);
            }

            // Update the observable collection
            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }
        }

        private async Task AddToCart(Product product)
        {
            if (product == null) return;

            try
            {
                await _databaseService.AddToCartAsync(_currentUser.UserId, product.ProductId, 1); // Add 1 quantity
                MessageBox.Show($"{product.Name} добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewCart()
        {
            _appViewModel.NavigateToCart();
        }

        private void ViewProfile()
        {
            _appViewModel.NavigateToProfile();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}