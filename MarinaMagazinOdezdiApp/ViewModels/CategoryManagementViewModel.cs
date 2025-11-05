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

using System;
using MarinaMagazinOdezdiApp.Views;

namespace MarinaMagazinOdezdiApp.ViewModels
{
    public class CategoryManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Category> Categories { get; set; }

        public event Action CategoriesChanged;

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public CategoryManagementViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Categories = new ObservableCollection<Category>();

            AddCategoryCommand = new AsyncRelayCommand(AddCategory);
            EditCategoryCommand = new AsyncRelayCommand<Category>(EditCategory);
            DeleteCategoryCommand = new AsyncRelayCommand<Category>(DeleteCategory);
        }

        public async Task LoadCategoriesAsync()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private async Task AddCategory()
        {
            AddEditCategoryWindow window = null;
            var viewModel = new AddEditCategoryViewModel(result => 
            {
                window.DialogResult = result;
                window.Close();
            });

            window = new AddEditCategoryWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                try
                {
                    var newCategory = new Category { CategoryName = viewModel.CategoryName };
                    await _databaseService.AddCategoryAsync(newCategory);
                    await LoadCategoriesAsync();
                    CategoriesChanged?.Invoke();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при добавлении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task EditCategory(Category category)
        {
            if (category == null) return;

            AddEditCategoryWindow window = null;
            var viewModel = new AddEditCategoryViewModel(result =>
            {
                window.DialogResult = result;
                window.Close();
            }, category);

            window = new AddEditCategoryWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                try
                {
                    category.CategoryName = viewModel.CategoryName;
                    await _databaseService.UpdateCategoryAsync(category);
                    await LoadCategoriesAsync();
                    CategoriesChanged?.Invoke();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при редактировании категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DeleteCategory(Category category)
        {
            if (category == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить категорию '{category.CategoryName}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _databaseService.DeleteCategoryAsync(category.CategoryId);
                    await LoadCategoriesAsync(); // Refresh the list
                    CategoriesChanged?.Invoke();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
