using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using MarinaMagazinOdezdiApp.Commands;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ProductsViewModel _productsViewModel;

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public ICommand LogoutCommand { get; }

        public ApplicationViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LogoutCommand = new RelayCommand(Logout);
            CurrentViewModel = new LoginRegisterViewModel(_databaseService, OnLoginSuccess);
        }

        private async void OnLoginSuccess(User user)
        {
            CurrentUser = user;

            if (user.Role == "Admin")
            {
                CurrentViewModel = new AdminViewModel(_databaseService);
            }
            else
            {
                if (_productsViewModel == null)
                {
                    _productsViewModel = new ProductsViewModel(this, _databaseService, CurrentUser);
                }
                await _productsViewModel.InitializeAsync();
                CurrentViewModel = _productsViewModel;
            }
        }

        private void Logout()
        {
            CurrentUser = null;
            _productsViewModel = null; // Clear the products view model
            CurrentViewModel = new LoginRegisterViewModel(_databaseService, OnLoginSuccess);
        }

        public void NavigateToCart()
        {
            CurrentViewModel = new CartViewModel(this, _databaseService, CurrentUser);
        }

        public void NavigateToCheckout(List<CartItem> cartItems)
        {
            CurrentViewModel = new CheckoutViewModel(this, _databaseService, CurrentUser, cartItems);
        }

        public async void NavigateToProducts()
        {
            if (_productsViewModel != null)
            {
                await _productsViewModel.InitializeAsync(); // Re-initialize to refresh data
            }
            CurrentViewModel = _productsViewModel;
        }

        public async void NavigateToProfile()
        {
            try
            {
                var profileViewModel = new UserProfileViewModel(this, _databaseService, CurrentUser);
                await profileViewModel.LoadOrdersAsync();
                CurrentViewModel = profileViewModel;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Произошла ошибка при загрузке профиля: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
