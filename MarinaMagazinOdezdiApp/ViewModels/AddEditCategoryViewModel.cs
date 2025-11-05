using MarinaMagazinOdezdiApp.Commands;
using MarinaMagazinOdezdiApp.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class AddEditCategoryViewModel : INotifyPropertyChanged
    {
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged();
            }
        }

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

        public AddEditCategoryViewModel(Action<bool> closeAction, Category category = null)
        {
            _closeAction = closeAction;

            if (category != null)
            {
                Title = "Редактировать категорию";
                CategoryName = category.CategoryName;
            }
            else
            {
                Title = "Добавить категорию";
            }

            SaveCommand = new RelayCommand(OnSave, CanSave);
        }

        private void OnSave()
        {
            _closeAction(true);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(CategoryName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
