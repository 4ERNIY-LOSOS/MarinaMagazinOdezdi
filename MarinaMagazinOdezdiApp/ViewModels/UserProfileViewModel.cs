using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationViewModel _appViewModel;
        private readonly DatabaseService _databaseService;

        public User CurrentUser { get; }
        public ObservableCollection<Order> Orders { get; set; }

        public ICommand GoBackCommand { get; }

        public UserProfileViewModel(ApplicationViewModel appViewModel, DatabaseService databaseService, User currentUser)
        {
            _appViewModel = appViewModel;
            _databaseService = databaseService;
            CurrentUser = currentUser;
            Orders = new ObservableCollection<Order>();

            GoBackCommand = new RelayCommand(() => _appViewModel.NavigateToProducts());
        }

        public async Task LoadOrdersAsync()
        {
            var orders = await _databaseService.GetOrdersForUserAsync(CurrentUser.UserId);
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
