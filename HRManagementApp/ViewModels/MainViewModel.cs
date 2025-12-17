using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Core;
using HRManagementApp.Views;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string activeViewName = "Employees";

    [ObservableProperty]
    private User currentUser;

    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    public bool IsAdmin => CurrentUser.Role == UserRole.Admin;

    // Reusable view instances
    private readonly EmployeeView _employeeView;
    private readonly DepartmentView _departmentView;
    private readonly AttendanceView _attendanceView;
    private readonly LeaveView _leaveView;
    private readonly PayrollView _payrollView;
    private UserManagementView? _userManagementView;

    [RelayCommand]
    private void ShowEmployees()
    {
        CurrentView = _employeeView;
        ActiveViewName = "Employees";
    }

    [RelayCommand]
    private void ShowDepartments()
    {
        CurrentView = _departmentView;
        ActiveViewName = "Departments";
    }

    [RelayCommand]
    private void ShowAttendance()
    {
        CurrentView = _attendanceView;
        ActiveViewName = "Attendance";
    }

    [RelayCommand]
    private void ShowLeave()
    {
        CurrentView = _leaveView;
        ActiveViewName = "Leave";
    }

    [RelayCommand]
    private void ShowPayroll()
    {
        CurrentView = _payrollView;
        ActiveViewName = "Payroll";
    }

    [RelayCommand]
    private void ShowUserManagement()
    {
        if (!IsAdmin)
        {
            MessageBox.Show("Only administrators can access user management.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _userManagementView ??= new UserManagementView();
        CurrentView = _userManagementView;
        ActiveViewName = "Users";
    }

    [RelayCommand]
    private void Logout()
    {
        var result = MessageBox.Show(
            "Are you sure you want to logout?", 
            "Logout", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Close main window
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }
    }

    public MainViewModel(User currentUser)
    {
        CurrentUser = currentUser;
        WelcomeMessage = $"Welcome, {currentUser.FullName} ({currentUser.Role})";

        // Initialize all views once
        _employeeView = new EmployeeView();
        _departmentView = new DepartmentView();
        _attendanceView = new AttendanceView();
        _leaveView = new LeaveView();
        _payrollView = new PayrollView();
        
        ShowEmployees();
    }
}


