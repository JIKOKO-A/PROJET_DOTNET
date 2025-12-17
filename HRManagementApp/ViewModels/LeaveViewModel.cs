using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Data;
using HRManagementApp.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class LeaveViewModel : ObservableObject
{
    private readonly HRDbContext? _context;

    [ObservableProperty]
    private ObservableCollection<LeaveRequest> leaveRequests = new();

    [ObservableProperty]
    private ObservableCollection<Employee> employees = new();

    [ObservableProperty]
    private LeaveRequest? selectedLeaveRequest;

    [ObservableProperty]
    private bool isEditMode;

    public LeaveViewModel()
    {
        try
        {
            _context = new HRDbContext();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize LeaveViewModel: {ex.Message}",
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
            LeaveRequests.Clear();
            var leaveRequestsList = _context.LeaveRequests
                .Include(l => l.Employee)
                .OrderByDescending(l => l.StartDate)
                .ToList();
            foreach (var leave in leaveRequestsList)
            {
                LeaveRequests.Add(leave);
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
    private void AddLeaveRequest()
    {
        SelectedLeaveRequest = new LeaveRequest
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Status = LeaveStatus.Pending
        };
        IsEditMode = true;
    }

    [RelayCommand]
    private void EditLeaveRequest()
    {
        if (SelectedLeaveRequest != null)
        {
            IsEditMode = true;
        }
    }

    [RelayCommand]
    private void ApproveLeave()
    {
        if (SelectedLeaveRequest == null) return;

        SelectedLeaveRequest.Status = LeaveStatus.Approved;
        SaveLeaveRequest();
    }

    [RelayCommand]
    private void RejectLeave()
    {
        if (SelectedLeaveRequest == null) return;

        SelectedLeaveRequest.Status = LeaveStatus.Rejected;
        SaveLeaveRequest();
    }

    [RelayCommand]
    private void SaveLeaveRequest()
    {
        if (SelectedLeaveRequest == null || _context == null) return;

        try
        {
            if (SelectedLeaveRequest.Id == 0)
            {
                _context.LeaveRequests.Add(SelectedLeaveRequest);
            }
            else
            {
                _context.LeaveRequests.Update(SelectedLeaveRequest);
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedLeaveRequest = null;
            MessageBox.Show("Leave request saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving leave request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteLeaveRequest()
    {
        if (SelectedLeaveRequest == null || _context == null) return;

        var result = MessageBox.Show(
            "Are you sure you want to delete this leave request?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _context.LeaveRequests.Remove(SelectedLeaveRequest);
                _context.SaveChanges();
                LoadData();
                SelectedLeaveRequest = null;
                MessageBox.Show("Leave request deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting leave request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        SelectedLeaveRequest = null;
        LoadData();
    }
}


