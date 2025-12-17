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
        if (SelectedLeaveRequest == null)
        {
            MessageBox.Show("Please select a leave request to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedLeaveRequest.Id == 0)
        {
            MessageBox.Show("Cannot edit a new leave request. Please save it first.", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Create a clean copy for editing to avoid navigation property issues
        if (_context != null)
        {
            var leaveFromDb = _context.LeaveRequests
                .AsNoTracking()
                .FirstOrDefault(l => l.Id == SelectedLeaveRequest.Id);
            
            if (leaveFromDb != null)
            {
                SelectedLeaveRequest = new LeaveRequest
                {
                    Id = leaveFromDb.Id,
                    EmployeeId = leaveFromDb.EmployeeId,
                    StartDate = leaveFromDb.StartDate,
                    EndDate = leaveFromDb.EndDate,
                    LeaveType = leaveFromDb.LeaveType,
                    Status = leaveFromDb.Status,
                    Reason = leaveFromDb.Reason
                };
            }
        }

        IsEditMode = true;
    }

    [RelayCommand]
    private void ApproveLeave()
    {
        if (SelectedLeaveRequest == null || _context == null) return;

        try
        {
            // Reload leave request from database to ensure it's tracked
            var leaveToUpdate = _context.LeaveRequests.Find(SelectedLeaveRequest.Id);
            if (leaveToUpdate != null)
            {
                leaveToUpdate.Status = LeaveStatus.Approved;
                _context.SaveChanges();
                LoadData();
                MessageBox.Show("Leave request approved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Leave request not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            MessageBox.Show($"Error approving leave request: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error approving leave request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RejectLeave()
    {
        if (SelectedLeaveRequest == null || _context == null) return;

        try
        {
            // Reload leave request from database to ensure it's tracked
            var leaveToUpdate = _context.LeaveRequests.Find(SelectedLeaveRequest.Id);
            if (leaveToUpdate != null)
            {
                leaveToUpdate.Status = LeaveStatus.Rejected;
                _context.SaveChanges();
                LoadData();
                MessageBox.Show("Leave request rejected successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Leave request not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            MessageBox.Show($"Error rejecting leave request: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error rejecting leave request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SaveLeaveRequest()
    {
        if (SelectedLeaveRequest == null || _context == null) return;

        // Validation
        if (SelectedLeaveRequest.EmployeeId == 0)
        {
            MessageBox.Show("Please select an employee.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedLeaveRequest.StartDate > SelectedLeaveRequest.EndDate)
        {
            MessageBox.Show("Start date cannot be after end date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (SelectedLeaveRequest.Id == 0)
            {
                // New leave request - create a clean entity
                var newLeaveRequest = new LeaveRequest
                {
                    EmployeeId = SelectedLeaveRequest.EmployeeId,
                    StartDate = SelectedLeaveRequest.StartDate,
                    EndDate = SelectedLeaveRequest.EndDate,
                    LeaveType = SelectedLeaveRequest.LeaveType,
                    Status = SelectedLeaveRequest.Status,
                    Reason = SelectedLeaveRequest.Reason
                };
                _context.LeaveRequests.Add(newLeaveRequest);
            }
            else
            {
                // Update existing leave request - reload from database to avoid tracking issues
                var leaveToUpdate = _context.LeaveRequests.Find(SelectedLeaveRequest.Id);
                if (leaveToUpdate == null)
                {
                    MessageBox.Show("Leave request not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update only scalar properties
                leaveToUpdate.EmployeeId = SelectedLeaveRequest.EmployeeId;
                leaveToUpdate.StartDate = SelectedLeaveRequest.StartDate;
                leaveToUpdate.EndDate = SelectedLeaveRequest.EndDate;
                leaveToUpdate.LeaveType = SelectedLeaveRequest.LeaveType;
                leaveToUpdate.Status = SelectedLeaveRequest.Status;
                leaveToUpdate.Reason = SelectedLeaveRequest.Reason;
            }

            _context.SaveChanges();
            LoadData();
            IsEditMode = false;
            SelectedLeaveRequest = null;
            MessageBox.Show("Leave request saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            MessageBox.Show($"Error saving leave request: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Reload leave request from database to ensure it's tracked
                var leaveToDelete = _context.LeaveRequests.Find(SelectedLeaveRequest.Id);
                if (leaveToDelete != null)
                {
                    _context.LeaveRequests.Remove(leaveToDelete);
                    _context.SaveChanges();
                    LoadData();
                    SelectedLeaveRequest = null;
                    MessageBox.Show("Leave request deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Leave request not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Error deleting leave request: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


