# HR Management System

A modern Windows desktop application built with WPF and .NET 10 for managing human resources operations.

## Features

### 🔐 Authentication & Authorization
- **Role-based access control** with Admin and HR user roles
- Secure password hashing (SHA-256)
- User management (Admin only)

### 👥 Employee Management
- Add, update, and delete employees
- Track employee details (name, email, phone, salary, position)
- Department assignment
- Input validation and data integrity

### 🏢 Department Management
- Create and manage departments
- Assign department managers
- View department employees

### ⏰ Attendance Tracking
- Check-in/check-out functionality
- Track hours worked
- View attendance history by employee

### 🏖️ Leave Management
- Submit leave requests
- Approve/reject leave requests
- Track leave balances
- Multiple leave types support

### 💰 Payroll Processing
- Calculate employee payroll
- Automatic deductions (tax, insurance)
- Performance bonuses
- Monthly payroll reports

## Technology Stack

- **Framework:** .NET 10
- **UI:** WPF (Windows Presentation Foundation)
- **Database:** SQLite with Entity Framework Core
- **Architecture:** MVVM (Model-View-ViewModel)
- **MVVM Toolkit:** CommunityToolkit.Mvvm

## Project Structure

```
HRManagementApp/
├── HRManagementApp.Core/          # Domain models
│   ├── User.cs
│   ├── Employee.cs
│   ├── Department.cs
│   ├── Attendance.cs
│   ├── LeaveRequest.cs
│   └── Payroll.cs
├── HRManagementApp.Data/          # Data access layer
│   ├── HRDbContext.cs
│   ├── DatabaseInitializer.cs
│   └── AuthenticationService.cs
└── HRManagementApp/               # WPF application
    ├── ViewModels/                # MVVM ViewModels
    ├── Views/                     # XAML Views
    └── Converters/                # Value converters
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Windows 10/11
- Visual Studio 2022 or VS Code (with C# extension)

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd HRManagementApp
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project HRManagementApp/HRManagementApp.csproj
   ```

### Default Login Credentials

**Admin Account:**
- Username: `admin`
- Password: `admin123`
- Full access including user management

**HR Account:**
- Username: `hr`
- Password: `hr123`
- Access to HR operations (no user management)

⚠️ **Important:** Change these default passwords immediately in production!

## Usage

### First Login
1. Run the application
2. Login window appears
3. Enter credentials (see above)
4. Main window opens with role-appropriate features

### Managing Users (Admin Only)
1. Click "Users" button in navigation
2. View all system users
3. Add new users with specific roles
4. Update or deactivate users as needed

### Managing Employees
1. Click "Employees" in navigation
2. Fill in employee details in the form
3. Select department from dropdown
4. Click "Add Employee"
5. Edit or delete existing employees

### Tracking Attendance
1. Click "Attendance" in navigation
2. Select employee
3. Click "Check In" to record start time
4. Hours worked calculated automatically
5. View attendance history in the grid

### Processing Payroll
1. Click "Payroll" in navigation
2. Select employee and month/year
3. Click "Calculate Payroll"
4. System calculates:
   - Base salary
   - Deductions (tax, insurance)
   - Bonuses (attendance-based)
   - Net salary

## Database

The application uses SQLite for data storage. The database file `HRDatabase.db` is created automatically in the application directory on first run.

### Initial Data
The database is seeded with:
- 2 default users (admin, hr)
- 4 sample departments
- 3 sample employees

### Database Schema
- **Users** - System users and authentication
- **Employees** - Employee information
- **Departments** - Company departments
- **Attendances** - Attendance records
- **LeaveRequests** - Leave requests
- **Payrolls** - Payroll records

## Security Features

✅ Password hashing (SHA-256)  
✅ Role-based access control  
✅ Input validation  
✅ SQL injection prevention (Entity Framework)  
✅ User activation/deactivation  

### Production Security Recommendations

For production deployment, consider:
1. Stronger password hashing (bcrypt, Argon2)
2. Password complexity requirements
3. Password expiration policies
4. Account lockout after failed attempts
5. Audit logging for all user actions
6. HTTPS for network communication
7. Multi-factor authentication
8. Regular security updates

## Troubleshooting

### Database Issues
If you encounter database errors:
```bash
# Delete the database file
Remove-Item "HRManagementApp/bin/Debug/net10.0-windows/HRDatabase.db"

# Rebuild and run
dotnet build
dotnet run --project HRManagementApp/HRManagementApp.csproj
```

### Build Errors
If build fails due to locked files, stop all running instances:
```bash
taskkill /F /IM HRManagementApp.exe
dotnet build
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## License

[Specify your license here]

## Support

For issues and questions, please open an issue on the repository.

---

Built with ❤️ using .NET and WPF

