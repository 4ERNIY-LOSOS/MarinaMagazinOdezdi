using MarinaMagazinOdezdiApp.Services;
using MarinaMagazinOdezdiApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System;
using MarinaMagazinOdezdiApp.Commands;
using System.Windows.Controls; // Required for PasswordBox

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class LoginRegisterViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        // Properties for Login
        private string _loginEmail;
        public string LoginEmail
        {
            get => _loginEmail;
            set => SetProperty(ref _loginEmail, value);
        }

        // We no longer need a password property here, it will be passed directly

        // Properties for Registration
        private string _registerEmail;
        public string RegisterEmail
        {
            get => _registerEmail;
            set => SetProperty(ref _registerEmail, value);
        }

        private string _registerFirstName;
        public string RegisterFirstName
        {
            get => _registerFirstName;
            set => SetProperty(ref _registerFirstName, value);
        }

        private string _registerLastName;
        public string RegisterLastName
        {
            get => _registerLastName;
            set => SetProperty(ref _registerLastName, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private readonly Action<User> _onLoginSuccess;

        public LoginRegisterViewModel(DatabaseService databaseService, Action<User> onLoginSuccess)
        {
            _databaseService = databaseService;
            _onLoginSuccess = onLoginSuccess;
            LoginCommand = new AsyncRelayCommand<PasswordBox>(Login);
            RegisterCommand = new AsyncRelayCommand<PasswordBox>(Register);
        }

        public async Task Login(PasswordBox passwordBox)
        {
            if (string.IsNullOrWhiteSpace(LoginEmail) || passwordBox == null || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Пожалуйста, введите Email и пароль для входа.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User user = await _databaseService.LoginUserAsync(LoginEmail.Trim(), passwordBox.Password);
            if (user != null)
            {
                _onLoginSuccess?.Invoke(user);
            }
            else
            {
                MessageBox.Show("Неверный Email или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Register(PasswordBox passwordBox)
        {
            if (string.IsNullOrWhiteSpace(RegisterEmail) || passwordBox == null || string.IsNullOrWhiteSpace(passwordBox.Password) ||
                string.IsNullOrWhiteSpace(RegisterFirstName) || string.IsNullOrWhiteSpace(RegisterLastName))
            {
                MessageBox.Show("Пожалуйста, заполните все поля для регистрации.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User newUser = await _databaseService.RegisterUserAsync(RegisterEmail.Trim(), passwordBox.Password, RegisterFirstName.Trim(), RegisterLastName.Trim(), "Customer");
                MessageBox.Show($"Пользователь {newUser.Email} успешно зарегистрирован! Теперь вы можете войти.", "Регистрация успешна", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка при регистрации: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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