using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Windows;

using MarinaMagazinOdezdiApp.ViewModels;
using MarinaMagazinOdezdiApp.Views;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<User> Users { get; private set; }

        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public UserManagementViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Users = new ObservableCollection<User>();

            EditUserCommand = new AsyncRelayCommand<User>(EditUser);
            DeleteUserCommand = new AsyncRelayCommand<User>(DeleteUser);
        }

        public async Task LoadUsersAsync()
        {
            var users = await _databaseService.GetUsersAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private async Task EditUser(User user)
        {
            if (user == null) return;

            // Create a copy for editing to avoid modifying the original object in the list directly
            var userCopy = new User
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };

            AddEditUserWindow window = null;
            var viewModel = new AddEditUserViewModel(userCopy, result =>
            {
                window.DialogResult = result;
                window.Close();
            });

            window = new AddEditUserWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                try
                {
                    await _databaseService.UpdateUserAsync(viewModel.User);
                    await LoadUsersAsync(); // Refresh the list
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при обновлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DeleteUser(User user)
        {
            if (user == null) return;

            if (MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{user.Email}'? Это действие нельзя отменить.", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _databaseService.DeleteUserAsync(user.UserId);
                    await LoadUsersAsync(); // Refresh the list
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
