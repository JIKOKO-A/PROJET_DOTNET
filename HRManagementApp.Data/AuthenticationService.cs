using HRManagementApp.Core;
using System.Security.Cryptography;
using System.Text;

namespace HRManagementApp.Data;

public class AuthenticationService
{
    private readonly HRDbContext _context;

    public AuthenticationService(HRDbContext context)
    {
        _context = context;
    }

    public User? Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
        if (user == null) return null;

        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash) return null;

        // Update last login date
        user.LastLoginDate = DateTime.Now;
        _context.SaveChanges();

        return user;
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool CreateUser(string username, string password, string fullName, string email, UserRole role)
    {
        // Check if username or email already exists
        if (_context.Users.Any(u => u.Username == username || u.Email == email))
        {
            return false;
        }

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            FullName = fullName,
            Email = email,
            Role = role,
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return true;
    }

    public bool ChangePassword(int userId, string oldPassword, string newPassword)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return false;

        var oldPasswordHash = HashPassword(oldPassword);
        if (user.PasswordHash != oldPasswordHash) return false;

        user.PasswordHash = HashPassword(newPassword);
        _context.SaveChanges();
        return true;
    }
}

