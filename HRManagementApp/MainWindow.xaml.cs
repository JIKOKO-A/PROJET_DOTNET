using HRManagementApp.Core;
using HRManagementApp.ViewModels;
using System.Windows;

namespace HRManagementApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(User currentUser)
    {
        InitializeComponent();
        DataContext = new MainViewModel(currentUser);
    }
}