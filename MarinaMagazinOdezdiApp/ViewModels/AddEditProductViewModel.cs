using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class AddEditProductViewModel : INotifyPropertyChanged
    {
        private Product _product;
        public Product Product
        {
            get => _product;
            set => SetProperty(ref _product, value);
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                if (value != null)
                {
                    Product.CategoryId = value.CategoryId;
                }
            }
        }

        public ObservableCollection<Category> Categories { get; set; }

        public ICommand SaveCommand { get; }

        public AddEditProductViewModel(Product product, ObservableCollection<Category> categories)
        {
            Product = product ?? new Product();
            Categories = categories;
            SaveCommand = new RelayCommand<Window>(Save, CanSave);

            if (Product.CategoryId > 0)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == Product.CategoryId);
            }
        }

        private bool CanSave(Window window)
        {
            return !string.IsNullOrWhiteSpace(Product.Name) &&
                   Product.Price > 0 &&
                   Product.StockQuantity >= 0 &&
                   SelectedCategory != null;
        }

        private void Save(Window window)
        {
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
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
