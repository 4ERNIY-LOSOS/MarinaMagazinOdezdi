using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class AddEditUserViewModel : INotifyPropertyChanged
    {
        private User _user;
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Roles { get; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        private readonly Action<bool> _closeAction;

        public AddEditUserViewModel(User user, Action<bool> closeAction)
        {
            _user = user;
            _closeAction = closeAction;

            Title = "Редактировать пользователя";
            Roles = new ObservableCollection<string> { "Customer", "Admin" };

            SaveCommand = new RelayCommand(OnSave, CanSave);
        }

        private void OnSave()
        {
            _closeAction(true);
        }

        private bool CanSave()
        {
            return User != null &&
                   !string.IsNullOrWhiteSpace(User.Email) &&
                   !string.IsNullOrWhiteSpace(User.FirstName) &&
                   !string.IsNullOrWhiteSpace(User.LastName) &&
                   !string.IsNullOrWhiteSpace(User.Role);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
