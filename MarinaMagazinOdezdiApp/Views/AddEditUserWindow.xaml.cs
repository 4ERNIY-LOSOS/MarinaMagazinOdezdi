using MarinaMagazinOdezdiApp.ViewModels;
using System.Windows;

namespace MarinaMagazinOdezdiApp.Views
{
    public partial class AddEditUserWindow : Window
    {
        public AddEditUserWindow(AddEditUserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
