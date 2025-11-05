using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class CartItemViewModel : INotifyPropertyChanged
    {
        private readonly CartItem _cartItem;
        private readonly Action<CartItemViewModel> _onQuantityChanged;
        private readonly Action<CartItemViewModel> _onRemoved;

        public CartItem CartItem => _cartItem;

        public string Name => _cartItem.Product.Name;
        public decimal Price => _cartItem.Product.Price;

        public int Quantity
        {
            get => _cartItem.Quantity;
            set
            {
                if (_cartItem.Quantity != value)
                {
                    _cartItem.Quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                    _onQuantityChanged?.Invoke(this);
                }
            }
        }

        public decimal Subtotal => Price * Quantity;

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveCommand { get; }

        public CartItemViewModel(CartItem cartItem, Action<CartItemViewModel> onQuantityChanged, Action<CartItemViewModel> onRemoved)
        {
            _cartItem = cartItem;
            _onQuantityChanged = onQuantityChanged;
            _onRemoved = onRemoved;

            IncreaseQuantityCommand = new RelayCommand(() => Quantity++);
            DecreaseQuantityCommand = new RelayCommand(() => { if (Quantity > 1) Quantity--; else _onRemoved(this); });
            RemoveCommand = new RelayCommand(() => _onRemoved(this));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
