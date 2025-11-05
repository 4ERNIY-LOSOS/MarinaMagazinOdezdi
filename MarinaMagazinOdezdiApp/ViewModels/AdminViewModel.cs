using MarinaMagazinOdezdiApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        public ProductManagementViewModel ProductManagementVM { get; }
        public CategoryManagementViewModel CategoryManagementVM { get; }
        public UserManagementViewModel UserManagementVM { get; }

        public AdminViewModel(DatabaseService databaseService)
        {
            ProductManagementVM = new ProductManagementViewModel(databaseService);
            CategoryManagementVM = new CategoryManagementViewModel(databaseService);
            UserManagementVM = new UserManagementViewModel(databaseService);

            CategoryManagementVM.CategoriesChanged += async () => await ProductManagementVM.LoadDataAsync();

            _ = LoadDataAsync(); // Asynchronously load data
        }

        private async Task LoadDataAsync()
        {
            await ProductManagementVM.LoadDataAsync();
            await CategoryManagementVM.LoadCategoriesAsync();
            await UserManagementVM.LoadUsersAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
