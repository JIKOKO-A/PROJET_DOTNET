using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Data;
using HRManagementApp.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class PayrollViewModel : ObservableObject
{
    private readonly HRDbContext? _context;

    [ObservableProperty]
    private ObservableCollection<Payroll> payrolls = new();

    [ObservableProperty]
    private ObservableCollection<Employee> employees = new();

    [ObservableProperty]
    private Payroll? selectedPayroll;

    [ObservableProperty]
    private Employee? selectedEmployee;

    [ObservableProperty]
    private int selectedMonth = DateTime.Now.Month;

    [ObservableProperty]
    private int selectedYear = DateTime.Now.Year;

    [ObservableProperty]
    private bool isEditMode;

    public PayrollViewModel()
    {
        try
        {
            _context = new HRDbContext();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize PayrollViewModel: {ex.Message}",
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
            Payrolls.Clear();
            var payrollsList = _context.Payrolls
                .Include(p => p.Employee)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToList();
            foreach (var payroll in payrollsList)
            {
                Payrolls.Add(payroll);
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
    private void AddPayroll()
    {
        SelectedPayroll = new Payroll
        {
            Month = SelectedMonth,
            Year = SelectedYear
        };
        IsEditMode = true;
    }

    [RelayCommand]
    private void CalculatePayroll()
    {
        if (SelectedEmployee == null || _context == null)
        {
            MessageBox.Show("Please select an employee first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existingPayroll = _context.Payrolls
            .FirstOrDefault(p => p.EmployeeId == SelectedEmployee.Id && 
                                p.Month == SelectedMonth && 
                                p.Year == SelectedYear);

        if (existingPayroll != null)
        {
            MessageBox.Show("Payroll for this employee and period already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Calculate base salary
        decimal baseSalary = SelectedEmployee.Salary;

        // Calculate deductions (example: 10% for taxes, 5% for insurance)
        decimal taxDeduction = baseSalary * 0.10m;
        decimal insuranceDeduction = baseSalary * 0.05m;
        decimal totalDeductions = taxDeduction + insuranceDeduction;

        // Calculate bonuses (example: based on attendance)
        var attendanceDays = _context.Attendances
            .Count(a => a.EmployeeId == SelectedEmployee.Id &&
                       a.Date.Year == SelectedYear &&
                       a.Date.Month == SelectedMonth &&
                       a.HoursWorked >= 8);

        decimal bonus = attendanceDays * 50; // $50 per full day

        // Calculate net salary
        decimal netSalary = baseSalary - totalDeductions + bonus;

        // Store employee name before LoadData clears selection
        var employeeName = SelectedEmployee.FullName;

        var payroll = new Payroll
        {
            EmployeeId = SelectedEmployee.Id,
            Month = SelectedMonth,
            Year = SelectedYear,
            BaseSalary = baseSalary,
            Deductions = totalDeductions,
            Bonuses = bonus,
            NetSalary = netSalary
        };

        _context.Payrolls.Add(payroll);
        _context.SaveChanges();
        LoadData();
         MessageBox.Show($"Payroll calculated for {employeeName}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void SavePayroll()
    {
        if (SelectedPayroll == null || _context == null) return;

        try
        {
            if (SelectedPayroll.Id == 0)
            {
                _context.Payrolls.Add(SelectedPayroll);
            }
            else
            {
                _context.Payrolls.Update(SelectedPayroll);
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedPayroll = null;
            MessageBox.Show("Payroll saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving payroll: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeletePayroll()
    {
        if (SelectedPayroll == null || _context == null) return;

        var result = MessageBox.Show(
            "Are you sure you want to delete this payroll record?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _context.Payrolls.Remove(SelectedPayroll);
                _context.SaveChanges();
                LoadData();
                SelectedPayroll = null;
                MessageBox.Show("Payroll deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payroll: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        SelectedPayroll = null;
        LoadData();
    }
}


