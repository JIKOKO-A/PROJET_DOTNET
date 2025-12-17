using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Data;
using HRManagementApp.Core;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HRDbContext _context;
    private readonly AuthenticationService _authService;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public User? CurrentUser { get; private set; }

    public LoginViewModel()
    {
        _context = new HRDbContext();
        _authService = new AuthenticationService(_context);
    }

    [RelayCommand]
    private void Login(object parameter)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter username";
            return;
        }

        if (parameter is not System.Windows.Controls.PasswordBox passwordBox)
        {
            ErrorMessage = "Password is required";
            return;
        }

        var password = passwordBox.Password;
        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter password";
            return;
        }

        IsLoading = true;

        try
        {
            CurrentUser = _authService.Login(Username, password);

            if (CurrentUser == null)
            {
                ErrorMessage = "Invalid username or password";
                IsLoading = false;
                return;
            }

            // Close login window and open main window
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = new MainWindow(CurrentUser);
                mainWindow.Show();

                // Close login window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.LoginWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
            IsLoading = false;
        }
    }
}

