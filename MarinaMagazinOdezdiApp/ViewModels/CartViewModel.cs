using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class CartViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationViewModel _appViewModel;
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;

        public ObservableCollection<CartItemViewModel> CartItems { get; set; }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        public ICommand BackToProductsCommand { get; }
        public ICommand ProceedToCheckoutCommand { get; }

        public CartViewModel(ApplicationViewModel appViewModel, DatabaseService databaseService, User currentUser)
        {
            _appViewModel = appViewModel;
            _databaseService = databaseService;
            _currentUser = currentUser;
            CartItems = new ObservableCollection<CartItemViewModel>();

            BackToProductsCommand = new RelayCommand(NavigateToProducts);
            ProceedToCheckoutCommand = new RelayCommand(ProceedToCheckout, () => CartItems.Any());
        }

        private void NavigateToProducts()
        {
            _appViewModel.NavigateToProducts();
        }

        public async Task LoadCartItemsAsync()
        {
            var cartItems = await _databaseService.GetCartItemsAsync(_currentUser.UserId);
            CartItems.Clear();
            foreach (var item in cartItems)
            {
                CartItems.Add(new CartItemViewModel(item, OnQuantityChanged, OnItemRemoved));
            }
            CalculateTotal();
        }

        private async void OnQuantityChanged(CartItemViewModel item)
        {
            await _databaseService.UpdateCartItemQuantityAsync(item.CartItem.CartItemId, item.Quantity);
            CalculateTotal();
        }

        private async void OnItemRemoved(CartItemViewModel item)
        {
            await _databaseService.RemoveFromCartAsync(item.CartItem.CartItemId);
            CartItems.Remove(item);
            CalculateTotal();
        }

        private void ProceedToCheckout()
        {
            var cartItems = CartItems.Select(vm => vm.CartItem).ToList();
            _appViewModel.NavigateToCheckout(cartItems);
        }

        private void CalculateTotal()
        {
            TotalPrice = CartItems.Sum(item => item.Subtotal);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
