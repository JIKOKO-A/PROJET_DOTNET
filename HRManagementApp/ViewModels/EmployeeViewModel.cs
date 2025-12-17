using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Data;
using HRManagementApp.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class EmployeeViewModel : ObservableObject
{
    private readonly HRDbContext? _context;

    [ObservableProperty]
    private ObservableCollection<Employee> employees = new();

    [ObservableProperty]
    private ObservableCollection<Department> departments = new();

    [ObservableProperty]
    private Employee? selectedEmployee;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isEditMode;

    public EmployeeViewModel()
    {
        try
        {
            _context = new HRDbContext();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize EmployeeViewModel: {ex.Message}",
                "Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadData()
    {
        if (_context == null) return;
        
        try
        {
            Employees.Clear();
            var employeesList = _context.Employees
                .Include(e => e.Department)
                .ToList();
            foreach (var emp in employeesList)
            {
                Employees.Add(emp);
            }

            Departments.Clear();
            var departmentsList = _context.Departments.ToList();
            foreach (var dept in departmentsList)
            {
                Departments.Add(dept);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to load data: {ex.Message}",
                "Data Load Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddEmployee()
    {
        SelectedEmployee = new Employee
        {
            HireDate = DateTime.Now
        };
        IsEditMode = true;
    }

    [RelayCommand]
    private void EditEmployee()
    {
        if (SelectedEmployee != null)
        {
            IsEditMode = true;
        }
    }

    [RelayCommand]
    private void SaveEmployee()
    {
        if (SelectedEmployee == null || _context == null) return;

        // Validation
        if (string.IsNullOrWhiteSpace(SelectedEmployee.FirstName))
        {
            MessageBox.Show("First name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedEmployee.LastName))
        {
            MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedEmployee.Email))
        {
            MessageBox.Show("Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Email format validation
        if (!System.Text.RegularExpressions.Regex.IsMatch(SelectedEmployee.Email, 
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check for duplicate email (excluding current employee)
        var existingEmployee = _context.Employees
            .FirstOrDefault(e => e.Email == SelectedEmployee.Email && e.Id != SelectedEmployee.Id);
        if (existingEmployee != null)
        {
            MessageBox.Show("An employee with this email already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedEmployee.Salary < 0)
        {
            MessageBox.Show("Salary cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (SelectedEmployee.Id == 0)
            {
                _context.Employees.Add(SelectedEmployee);
            }
            else
            {
                _context.Employees.Update(SelectedEmployee);
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedEmployee = null;
            MessageBox.Show("Employee saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteEmployee()
    {
        if (SelectedEmployee == null || _context == null) return;

        var employeeName = SelectedEmployee.FullName; // Store name before deletion
        var result = MessageBox.Show(
            $"Are you sure you want to delete {employeeName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _context.Employees.Remove(SelectedEmployee);
                _context.SaveChanges();
                LoadData();
                SelectedEmployee = null;
                MessageBox.Show($"Employee {employeeName} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        SelectedEmployee = null;
        LoadData();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || _context == null)
        {
            LoadData();
            return;
        }

        Employees.Clear();
        var filtered = _context.Employees
            .Include(e => e.Department)
            .Where(e => e.FirstName.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                       e.LastName.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                       e.Email.Contains(value, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var emp in filtered)
        {
            Employees.Add(emp);
        }
    }
}


