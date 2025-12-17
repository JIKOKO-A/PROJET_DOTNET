using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Data;
using HRManagementApp.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class DepartmentViewModel : ObservableObject
{
    private readonly HRDbContext? _context;

    [ObservableProperty]
    private ObservableCollection<Department> departments = new();

    [ObservableProperty]
    private ObservableCollection<Employee> employees = new();

    [ObservableProperty]
    private Department? selectedDepartment;

    [ObservableProperty]
    private bool isEditMode;

    public DepartmentViewModel()
    {
        try
        {
            _context = new HRDbContext();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize DepartmentViewModel: {ex.Message}",
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
            Departments.Clear();
            var departmentsList = _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .ToList();
            foreach (var dept in departmentsList)
            {
                Departments.Add(dept);
            }

            Employees.Clear();
            var employeesList = _context.Employees.ToList();
            foreach (var emp in employeesList)
            {
                Employees.Add(emp);
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
    private void AddDepartment()
    {
        SelectedDepartment = new Department();
        IsEditMode = true;
    }

    [RelayCommand]
    private void EditDepartment()
    {
        if (SelectedDepartment != null)
        {
            IsEditMode = true;
        }
    }

    [RelayCommand]
    private void SaveDepartment()
    {
        if (SelectedDepartment == null || _context == null) return;

        // Validation
        if (string.IsNullOrWhiteSpace(SelectedDepartment.Name))
        {
            MessageBox.Show("Department name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check for duplicate name (excluding current department)
        var existingDepartment = _context.Departments
            .FirstOrDefault(d => d.Name == SelectedDepartment.Name && d.Id != SelectedDepartment.Id);
        if (existingDepartment != null)
        {
            MessageBox.Show("A department with this name already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (SelectedDepartment.Id == 0)
            {
                _context.Departments.Add(SelectedDepartment);
            }
            else
            {
                _context.Departments.Update(SelectedDepartment);
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedDepartment = null;
            MessageBox.Show("Department saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving department: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteDepartment()
    {
        if (SelectedDepartment == null) return;

        var departmentName = SelectedDepartment.Name; // Store name before deletion
        var result = MessageBox.Show(
            $"Are you sure you want to delete {departmentName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes && _context != null)
        {
            try
            {
                _context.Departments.Remove(SelectedDepartment);
                _context.SaveChanges();
                LoadData();
                SelectedDepartment = null;
                MessageBox.Show($"Department {departmentName} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting department: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        SelectedDepartment = null;
        if (_context != null)
        {
            LoadData();
        }
    }
}


