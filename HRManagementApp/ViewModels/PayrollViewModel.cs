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
    private ObservableCollection<Payroll> filteredPayrolls = new();

    [ObservableProperty]
    private ObservableCollection<Employee> employees = new();

    [ObservableProperty]
    private Payroll? selectedPayroll;

    [ObservableProperty]
    private Employee? selectedEmployee;

    private int _selectedMonth = DateTime.Now.Month;
    public int SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                FilterPayrolls();
            }
        }
    }

    private int _selectedYear = DateTime.Now.Year;
    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            // Validate year range (reasonable business years)
            if (value < 2000 || value > 2100)
            {
                return; // Ignore invalid years
            }
            if (SetProperty(ref _selectedYear, value))
            {
                FilterPayrolls();
            }
        }
    }

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private decimal taxRate = 10.0m; // Percentage

    [ObservableProperty]
    private decimal insuranceRate = 5.0m; // Percentage

    [ObservableProperty]
    private decimal bonusPerDay = 50.0m; // Amount per full workday

    private bool _filterByMonthYear = true;
    public bool FilterByMonthYear
    {
        get => _filterByMonthYear;
        set
        {
            if (SetProperty(ref _filterByMonthYear, value))
            {
                FilterPayrolls();
            }
        }
    }

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

            FilterPayrolls();
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

    private void FilterPayrolls()
    {
        FilteredPayrolls.Clear();
        
        if (FilterByMonthYear)
        {
            // Filter by selected month and year
            var filtered = Payrolls.Where(p => p.Month == SelectedMonth && p.Year == SelectedYear);
            foreach (var payroll in filtered)
            {
                FilteredPayrolls.Add(payroll);
            }
        }
        else
        {
            // Show all payrolls
            foreach (var payroll in Payrolls)
            {
                FilteredPayrolls.Add(payroll);
            }
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

        // Calculate deductions using customizable rates
        decimal taxDeduction = baseSalary * (TaxRate / 100m);
        decimal insuranceDeduction = baseSalary * (InsuranceRate / 100m);
        decimal totalDeductions = taxDeduction + insuranceDeduction;

        // Calculate bonuses based on attendance
        var attendanceDays = _context.Attendances
            .Count(a => a.EmployeeId == SelectedEmployee.Id &&
                       a.Date.Year == SelectedYear &&
                       a.Date.Month == SelectedMonth &&
                       a.HoursWorked >= 8);

        decimal bonus = attendanceDays * BonusPerDay;

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
        MessageBox.Show($"Payroll calculated for {employeeName}!\nBase Salary: {baseSalary:N2} MAD\nDeductions: {totalDeductions:N2} MAD\nBonuses: {bonus:N2} MAD\nNet Salary: {netSalary:N2} MAD", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void EditPayroll()
    {
        if (SelectedPayroll == null)
        {
            MessageBox.Show("Please select a payroll record to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedPayroll.Id == 0)
        {
            MessageBox.Show("Cannot edit a new payroll. Please save it first.", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Create a clean copy for editing to avoid navigation property issues
        if (_context != null)
        {
            try
            {
                var payrollFromDb = _context.Payrolls
                    .AsNoTracking()
                    .FirstOrDefault(p => p.Id == SelectedPayroll.Id);
                
                if (payrollFromDb != null)
                {
                    SelectedPayroll = new Payroll
                    {
                        Id = payrollFromDb.Id,
                        EmployeeId = payrollFromDb.EmployeeId,
                        Month = payrollFromDb.Month,
                        Year = payrollFromDb.Year,
                        BaseSalary = payrollFromDb.BaseSalary,
                        Deductions = payrollFromDb.Deductions,
                        Bonuses = payrollFromDb.Bonuses,
                        NetSalary = payrollFromDb.NetSalary
                    };
                    OnPropertyChanged(nameof(SelectedPayroll));
                    IsEditMode = true;
                }
                else
                {
                    MessageBox.Show("Payroll record not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payroll for editing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void RecalculatePayroll()
    {
        if (SelectedPayroll == null || _context == null)
        {
            MessageBox.Show("Please select a payroll record first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedPayroll.EmployeeId == 0)
        {
            MessageBox.Show("Please select an employee first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var employee = _context.Employees.Find(SelectedPayroll.EmployeeId);
        if (employee == null)
        {
            MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            // Recalculate base salary
            decimal baseSalary = employee.Salary;
            SelectedPayroll.BaseSalary = baseSalary;

            // Recalculate deductions
            decimal taxDeduction = baseSalary * (TaxRate / 100m);
            decimal insuranceDeduction = baseSalary * (InsuranceRate / 100m);
            decimal totalDeductions = taxDeduction + insuranceDeduction;
            SelectedPayroll.Deductions = totalDeductions;

            // Recalculate bonuses based on attendance
            var attendanceDays = _context.Attendances
                .Count(a => a.EmployeeId == SelectedPayroll.EmployeeId &&
                           a.Date.Year == SelectedPayroll.Year &&
                           a.Date.Month == SelectedPayroll.Month &&
                           a.HoursWorked >= 8);

            decimal bonus = attendanceDays * BonusPerDay;
            SelectedPayroll.Bonuses = bonus;

            // Recalculate net salary
            SelectedPayroll.NetSalary = baseSalary - totalDeductions + bonus;

            // Notify property changes
            OnPropertyChanged(nameof(SelectedPayroll));
            MessageBox.Show("Payroll recalculated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error recalculating payroll: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SavePayroll()
    {
        if (SelectedPayroll == null || _context == null) return;

        // Validation
        if (SelectedPayroll.EmployeeId == 0)
        {
            MessageBox.Show("Please select an employee.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedPayroll.Month < 1 || SelectedPayroll.Month > 12)
        {
            MessageBox.Show("Month must be between 1 and 12.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Always recalculate net salary before saving
        SelectedPayroll.NetSalary = SelectedPayroll.BaseSalary - SelectedPayroll.Deductions + SelectedPayroll.Bonuses;

        try
        {
            if (SelectedPayroll.Id == 0)
            {
                // New payroll - create a clean entity
                var newPayroll = new Payroll
                {
                    EmployeeId = SelectedPayroll.EmployeeId,
                    Month = SelectedPayroll.Month,
                    Year = SelectedPayroll.Year,
                    BaseSalary = SelectedPayroll.BaseSalary,
                    Deductions = SelectedPayroll.Deductions,
                    Bonuses = SelectedPayroll.Bonuses,
                    NetSalary = SelectedPayroll.NetSalary
                };
                _context.Payrolls.Add(newPayroll);
            }
            else
            {
                // Update existing payroll - reload from database to avoid tracking issues
                var payrollToUpdate = _context.Payrolls.Find(SelectedPayroll.Id);
                if (payrollToUpdate == null)
                {
                    MessageBox.Show("Payroll not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update only scalar properties
                payrollToUpdate.EmployeeId = SelectedPayroll.EmployeeId;
                payrollToUpdate.Month = SelectedPayroll.Month;
                payrollToUpdate.Year = SelectedPayroll.Year;
                payrollToUpdate.BaseSalary = SelectedPayroll.BaseSalary;
                payrollToUpdate.Deductions = SelectedPayroll.Deductions;
                payrollToUpdate.Bonuses = SelectedPayroll.Bonuses;
                payrollToUpdate.NetSalary = SelectedPayroll.NetSalary;
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedPayroll = null;
            MessageBox.Show("Payroll saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            MessageBox.Show($"Error saving payroll: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Reload payroll from database to ensure it's tracked
                var payrollToDelete = _context.Payrolls.Find(SelectedPayroll.Id);
                if (payrollToDelete != null)
                {
                    _context.Payrolls.Remove(payrollToDelete);
                    _context.SaveChanges();
                    LoadData();
                    SelectedPayroll = null;
                    MessageBox.Show("Payroll deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Payroll not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Error deleting payroll: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


