using HRManagementApp.Data;
using HRManagementApp.Views;
using System.Windows;

namespace HRManagementApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize database before any windows are created
        try
        {
            using var context = new HRDbContext();
            DatabaseInitializer.Initialize(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize database: {ex.Message}\n\nApplication will continue but may not function correctly.",
                "Database Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // Show login window
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}

