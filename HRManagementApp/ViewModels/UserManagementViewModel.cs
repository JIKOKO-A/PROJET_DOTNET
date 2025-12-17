using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HRManagementApp.Core;
using HRManagementApp.Data;
using System.Collections.ObjectModel;
using System.Windows;

namespace HRManagementApp.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    private readonly HRDbContext? _context;

    [ObservableProperty]
    private ObservableCollection<User> users = new();

    [ObservableProperty]
    private User? selectedUser;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private UserRole selectedRole = UserRole.HR;

    [ObservableProperty]
    private bool isActive = true;

    public Array UserRoles => Enum.GetValues(typeof(UserRole));

    public UserManagementViewModel()
    {
        try
        {
            _context = new HRDbContext();
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize User Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadUsers()
    {
        if (_context == null) return;

        try
        {
            Users = new ObservableCollection<User>(_context.Users.OrderBy(u => u.Username).ToList());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddUser()
    {
        if (_context == null) return;

        // Validation
        if (string.IsNullOrWhiteSpace(Username))
        {
            MessageBox.Show("Username is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            MessageBox.Show("Password is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            MessageBox.Show("Full name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            MessageBox.Show("Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check for duplicate username or email
        if (_context.Users.Any(u => u.Username == Username))
        {
            MessageBox.Show("Username already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_context.Users.Any(u => u.Email == Email))
        {
            MessageBox.Show("Email already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var user = new User
            {
                Username = Username,
                PasswordHash = AuthenticationService.HashPassword(Password),
                FullName = FullName,
                Email = Email,
                Role = SelectedRole,
                IsActive = IsActive,
                CreatedDate = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            LoadUsers();
            ClearForm();

            MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void UpdateUser()
    {
        if (_context == null || SelectedUser == null) return;

        // Validation
        if (string.IsNullOrWhiteSpace(Username))
        {
            MessageBox.Show("Username is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            MessageBox.Show("Full name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            MessageBox.Show("Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check for duplicate username or email (excluding current user)
        if (_context.Users.Any(u => u.Username == Username && u.Id != SelectedUser.Id))
        {
            MessageBox.Show("Username already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_context.Users.Any(u => u.Email == Email && u.Id != SelectedUser.Id))
        {
            MessageBox.Show("Email already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            SelectedUser.Username = Username;
            SelectedUser.FullName = FullName;
            SelectedUser.Email = Email;
            SelectedUser.Role = SelectedRole;
            SelectedUser.IsActive = IsActive;

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(Password))
            {
                SelectedUser.PasswordHash = AuthenticationService.HashPassword(Password);
            }

            _context.SaveChanges();
            LoadUsers();
            ClearForm();

            MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteUser()
    {
        if (_context == null || SelectedUser == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete user '{SelectedUser.Username}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _context.Users.Remove(SelectedUser);
                _context.SaveChanges();
                LoadUsers();
                ClearForm();

                MessageBox.Show("User deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void SelectUser(User user)
    {
        SelectedUser = user;
        Username = user.Username;
        Password = string.Empty; // Don't show password
        FullName = user.FullName;
        Email = user.Email;
        SelectedRole = user.Role;
        IsActive = user.IsActive;
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedUser = null;
        Username = string.Empty;
        Password = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
        SelectedRole = UserRole.HR;
        IsActive = true;
    }
}

