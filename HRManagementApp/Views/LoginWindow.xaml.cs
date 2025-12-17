using System.Windows;
using System.Windows.Input;

namespace HRManagementApp.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Trigger login command when Enter is pressed
            if (DataContext is ViewModels.LoginViewModel viewModel)
            {
                viewModel.LoginCommand.Execute(PasswordBox);
            }
        }
    }
}

