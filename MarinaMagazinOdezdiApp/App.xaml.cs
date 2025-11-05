using System.Windows;
using MarinaMagazinOdezdiApp.Services;
using MarinaMagazinOdezdiApp.ViewModels;
using MarinaMagazinOdezdiApp.Views;

namespace MarinaMagazinOdezdiApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create services and main view model
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=MarinaMagazinOdezdi;Integrated Security=True;TrustServerCertificate=True;";
            var databaseService = new DatabaseService(connectionString);
            var appViewModel = new ApplicationViewModel(databaseService);

            // Create the main shell window
            var shell = new ShellWindow
            {
                DataContext = appViewModel
            };

            shell.Show();
        }
    }
}
