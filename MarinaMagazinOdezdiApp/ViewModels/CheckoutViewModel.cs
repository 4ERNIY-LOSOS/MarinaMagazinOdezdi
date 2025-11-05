using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using MarinaMagazinOdezdiApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class CheckoutViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationViewModel _appViewModel;
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private readonly List<CartItem> _cartItems;

        public decimal TotalPrice { get; }

        // Address Properties
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }

        // Payment Properties
        public bool IsCardPayment { get; set; } = true;
        public bool IsCashPayment { get; set; }

        public ICommand PlaceOrderCommand { get; }
        public ICommand CancelCommand { get; }

        public CheckoutViewModel(ApplicationViewModel appViewModel, DatabaseService databaseService, User currentUser, List<CartItem> cartItems)
        {
            _appViewModel = appViewModel;
            _databaseService = databaseService;
            _currentUser = currentUser;
            _cartItems = cartItems;

            TotalPrice = _cartItems.Sum(item => item.Product.Price * item.Quantity);

            PlaceOrderCommand = new AsyncRelayCommand(PlaceOrder, CanPlaceOrder);
            CancelCommand = new RelayCommand(() => _appViewModel.NavigateToCart());
        }

        private bool CanPlaceOrder()
        {
            return !string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(HouseNumber);
        }

        private async Task PlaceOrder()
        {
            try
            {
                await _databaseService.PlaceOrderAsync(_currentUser.UserId, _cartItems, City, Street, HouseNumber);
                MessageBox.Show("Заказ успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _appViewModel.NavigateToProducts();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка оформления заказа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
