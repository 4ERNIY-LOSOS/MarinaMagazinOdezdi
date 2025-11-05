using MarinaMagazinOdezdiApp.ViewModels;
using System.Windows;

namespace MarinaMagazinOdezdiApp.Views
{
    public partial class AddEditCategoryWindow : Window
    {
        public AddEditCategoryWindow(AddEditCategoryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
