using MarinaMagazinOdezdiApp.ViewModels;
using System.Windows;
using System.Windows.Controls; // Added for UserControl

namespace MarinaMagazinOdezdiApp.Views
{
    public partial class CartView : UserControl
    {
        public CartView()
        {
            InitializeComponent();
            Loaded += async (s, e) => 
            {
                if (DataContext is CartViewModel viewModel)
                {
                    await viewModel.LoadCartItemsAsync();
                }
            };
        }
    }
}
