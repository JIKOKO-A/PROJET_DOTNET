using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Data;
using HRManagementApp.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class AttendanceViewModel : ObservableObject
{
    private readonly HRDbContext? _context;

    [ObservableProperty]
    private ObservableCollection<Attendance> attendances = new();

    [ObservableProperty]
    private ObservableCollection<Employee> employees = new();

    [ObservableProperty]
    private Attendance? selectedAttendance;

    [ObservableProperty]
    private Employee? selectedEmployee;

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Now;

    [ObservableProperty]
    private bool isEditMode;

    public AttendanceViewModel()
    {
        try
        {
            _context = new HRDbContext();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize AttendanceViewModel: {ex.Message}",
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
            Attendances.Clear();
            var attendancesList = _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .ToList();
            foreach (var att in attendancesList)
            {
                Attendances.Add(att);
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
    private void AddAttendance()
    {
        if (SelectedEmployee == null)
        {
            MessageBox.Show("Please select an employee first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedAttendance = new Attendance
        {
            EmployeeId = SelectedEmployee.Id,
            Date = SelectedDate,
            CheckIn = DateTime.Now
        };
        IsEditMode = true;
    }

    [RelayCommand]
    private void CheckIn()
    {
        if (SelectedEmployee == null || _context == null)
        {
            MessageBox.Show("Please select an employee first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var todayAttendance = _context.Attendances
            .FirstOrDefault(a => a.EmployeeId == SelectedEmployee.Id && a.Date.Date == DateTime.Now.Date);

        if (todayAttendance != null && todayAttendance.CheckIn.HasValue)
        {
            MessageBox.Show("Employee has already checked in today.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var employeeName = SelectedEmployee.FullName; // Store name before LoadData clears it
        var employeeId = SelectedEmployee.Id;

        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            Date = DateTime.Now.Date,
            CheckIn = DateTime.Now
        };

        _context.Attendances.Add(attendance);
        _context.SaveChanges();
        LoadData();
        MessageBox.Show($"{employeeName} checked in successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void CheckOut()
    {
        if (SelectedEmployee == null || _context == null)
        {
            MessageBox.Show("Please select an employee first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var todayAttendance = _context.Attendances
            .FirstOrDefault(a => a.EmployeeId == SelectedEmployee.Id && a.Date.Date == DateTime.Now.Date);

        if (todayAttendance == null || !todayAttendance.CheckIn.HasValue)
        {
            MessageBox.Show("Employee must check in first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (todayAttendance.CheckOut.HasValue)
        {
            MessageBox.Show("Employee has already checked out today.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var employeeName = SelectedEmployee.FullName; // Store name before LoadData clears it
        
        todayAttendance.CheckOut = DateTime.Now;
        if (todayAttendance.CheckIn.HasValue)
        {
            var timeSpan = todayAttendance.CheckOut.Value - todayAttendance.CheckIn.Value;
            todayAttendance.HoursWorked = timeSpan.TotalHours;
        }

        _context.SaveChanges();
        LoadData();
        MessageBox.Show($"{employeeName} checked out successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void SaveAttendance()
    {
        if (SelectedAttendance == null || _context == null) return;

        // Validation
        if (SelectedAttendance.EmployeeId == 0)
        {
            MessageBox.Show("Please select an employee.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (SelectedAttendance.CheckIn.HasValue && SelectedAttendance.CheckOut.HasValue)
            {
                var timeSpan = SelectedAttendance.CheckOut.Value - SelectedAttendance.CheckIn.Value;
                SelectedAttendance.HoursWorked = timeSpan.TotalHours;
            }

            if (SelectedAttendance.Id == 0)
            {
                _context.Attendances.Add(SelectedAttendance);
            }
            else
            {
                _context.Attendances.Update(SelectedAttendance);
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedAttendance = null;
            MessageBox.Show("Attendance saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteAttendance()
    {
        if (SelectedAttendance == null || _context == null) return;

        var result = MessageBox.Show(
            "Are you sure you want to delete this attendance record?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _context.Attendances.Remove(SelectedAttendance);
                _context.SaveChanges();
                LoadData();
                SelectedAttendance = null;
                MessageBox.Show("Attendance deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        SelectedAttendance = null;
        LoadData();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        LoadData();
    }
}


