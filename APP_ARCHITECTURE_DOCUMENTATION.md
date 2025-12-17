# HR Management App - Complete Architecture Documentation

## 📋 Table of Contents
1. [Application Overview](#application-overview)
2. [Page-by-Page Breakdown](#page-by-page-breakdown)
3. [File Roles and Functions](#file-roles-and-functions)
4. [Data Flow](#data-flow)

---

## 🏗️ Application Overview

**Architecture Pattern:** MVVM (Model-View-ViewModel)
**Framework:** .NET 10.0 with WPF
**Database:** SQLite with Entity Framework Core 10.0.0
**MVVM Toolkit:** CommunityToolkit.Mvvm 8.3.2

**Project Structure:**
```
HRManagementApp/
├── HRManagementApp.Core/          # Domain Models (Entities)
├── HRManagementApp.Data/          # Data Access Layer
└── HRManagementApp/               # WPF Application (Views + ViewModels)
```

---

## 📄 Page-by-Page Breakdown

### 1️⃣ **LOGIN PAGE** 🔐

#### **Files Used:**
- `Views/LoginWindow.xaml` - UI Layout
- `Views/LoginWindow.xaml.cs` - Code-behind
- `ViewModels/LoginViewModel.cs` - Business Logic
- `Data/AuthenticationService.cs` - Authentication Logic
- `Data/HRDbContext.cs` - Database Access
- `Core/User.cs` - User Model

#### **How It Works:**
1. User enters username and password
2. `LoginCommand` executes in `LoginViewModel`
3. Calls `AuthenticationService.Login()` which:
   - Finds user by username
   - Hashes entered password with SHA-256
   - Compares with stored password hash
   - Updates `LastLoginDate` if successful
4. If valid, opens `MainWindow` and closes `LoginWindow`
5. If invalid, shows error message

#### **Functions in LoginViewModel.cs:**
- `LoginViewModel()` - Constructor: Initializes database context and authentication service
- `Login(object parameter)` - Command: Validates input, calls auth service, opens main window

#### **Functions in AuthenticationService.cs:**
- `Login(string username, string password)` - Authenticates user, returns User object or null
- `HashPassword(string password)` - Static: Hashes password using SHA-256
- `CreateUser(...)` - Creates new user account
- `ChangePassword(...)` - Changes user password

---

### 2️⃣ **MAIN WINDOW** 🏠

#### **Files Used:**
- `MainWindow.xaml` - Main container layout
- `MainWindow.xaml.cs` - Code-behind
- `ViewModels/MainViewModel.cs` - Navigation logic
- All View files (EmployeeView, DepartmentView, etc.)

#### **How It Works:**
1. Receives `User` object from login
2. Creates `MainViewModel` with current user
3. Initializes all view instances once (singleton pattern)
4. Navigation buttons switch between views
5. Shows welcome message and logout button

#### **Functions in MainViewModel.cs:**
- `MainViewModel(User currentUser)` - Constructor: Initializes all views, sets welcome message
- `ShowEmployees()` - Command: Switches to EmployeeView
- `ShowDepartments()` - Command: Switches to DepartmentView
- `ShowAttendance()` - Command: Switches to AttendanceView
- `ShowLeave()` - Command: Switches to LeaveView
- `ShowPayroll()` - Command: Switches to PayrollView
- `ShowUserManagement()` - Command: Switches to UserManagementView (Admin only)
- `Logout()` - Command: Closes main window, opens login window

**Properties:**
- `CurrentView` - Currently displayed view
- `ActiveViewName` - Name of active view (for button highlighting)
- `CurrentUser` - Logged-in user
- `WelcomeMessage` - Welcome text
- `IsAdmin` - Computed property: true if user is Admin

---

### 3️⃣ **EMPLOYEE MANAGEMENT PAGE** 👥

#### **Files Used:**
- `Views/EmployeeView.xaml` - UI Layout
- `ViewModels/EmployeeViewModel.cs` - Business Logic
- `Core/Employee.cs` - Employee Model
- `Core/Department.cs` - Department Model (for dropdown)
- `Data/HRDbContext.cs` - Database Access

#### **How It Works:**
1. **Loads Data:** Fetches all employees with their departments
2. **Search:** Real-time filtering by FirstName, LastName, or Email
3. **Add:** Creates new employee, opens edit form
4. **Edit:** Select employee → opens edit form with pre-filled data
5. **Save:** Validates data → saves to database → refreshes list
6. **Delete:** Confirms → removes from database → refreshes list

#### **Functions in EmployeeViewModel.cs:**
- `EmployeeViewModel()` - Constructor: Initializes database, loads data
- `LoadData()` - Private: Loads employees and departments from database
- `AddEmployee()` - Command: Creates new employee, enables edit mode
- `EditEmployee()` - Command: Enables edit mode for selected employee
- `SaveEmployee()` - Command: Validates and saves employee (new or update)
- `DeleteEmployee()` - Command: Confirms and deletes employee
- `CancelEdit()` - Command: Cancels editing, clears selection
- `OnSearchTextChanged(string value)` - Partial: Filters employees by search text

**Properties:**
- `Employees` - Collection of all employees
- `Departments` - Collection of all departments (for dropdown)
- `SelectedEmployee` - Currently selected employee
- `SearchText` - Search filter text
- `IsEditMode` - Whether edit form is visible

#### **Employee Model (Core/Employee.cs):**
**Properties:**
- `Id` - Primary key
- `FirstName`, `LastName` - Name fields
- `Email`, `Phone` - Contact info
- `HireDate` - Employment start date
- `DepartmentId` - Foreign key to Department
- `Position` - Job title
- `Salary` - Monthly salary
- `Department` - Navigation property
- `Attendances`, `LeaveRequests`, `Payrolls` - Related collections
- `FullName` - Computed property: "FirstName LastName"

---

### 4️⃣ **DEPARTMENT MANAGEMENT PAGE** 🏢

#### **Files Used:**
- `Views/DepartmentView.xaml` - UI Layout
- `ViewModels/DepartmentViewModel.cs` - Business Logic
- `Core/Department.cs` - Department Model
- `Core/Employee.cs` - Employee Model (for manager selection)
- `Data/HRDbContext.cs` - Database Access

#### **How It Works:**
1. **Loads Data:** Fetches departments with managers and employee counts
2. **Add:** Creates new department, opens edit form
3. **Edit:** Select department → opens edit form
4. **Save:** Validates name uniqueness → saves → refreshes
5. **Delete:** Confirms → removes department (employees set to null)

#### **Functions in DepartmentViewModel.cs:**
- `DepartmentViewModel()` - Constructor: Initializes database, loads data
- `LoadData()` - Private: Loads departments with navigation properties
- `AddDepartment()` - Command: Creates new department, enables edit mode
- `EditDepartment()` - Command: Enables edit mode
- `SaveDepartment()` - Command: Validates and saves department (handles EF tracking)
- `DeleteDepartment()` - Command: Confirms and deletes department
- `CancelEdit()` - Command: Cancels editing

**Properties:**
- `Departments` - Collection of departments
- `Employees` - Collection of employees (for manager selection)
- `SelectedDepartment` - Currently selected department
- `IsEditMode` - Whether edit form is visible

#### **Department Model (Core/Department.cs):**
**Properties:**
- `Id` - Primary key
- `Name` - Department name
- `Description` - Optional description
- `ManagerId` - Foreign key to Employee (manager)
- `Manager` - Navigation property to Employee
- `Employees` - Collection of employees in department

---

### 5️⃣ **ATTENDANCE TRACKING PAGE** ⏰

#### **Files Used:**
- `Views/AttendanceView.xaml` - UI Layout
- `ViewModels/AttendanceViewModel.cs` - Business Logic
- `Core/Attendance.cs` - Attendance Model
- `Core/Employee.cs` - Employee Model
- `Data/HRDbContext.cs` - Database Access

#### **How It Works:**
1. **Select Employee:** Choose from dropdown
2. **Check In:** Records current time as CheckIn for today
3. **Check Out:** Records current time as CheckOut, calculates hours worked
4. **Add Manual:** Opens form to add attendance record manually
5. **View History:** DataGrid shows all attendance records
6. **Edit/Delete:** Select record → edit or delete

#### **Functions in AttendanceViewModel.cs:**
- `AttendanceViewModel()` - Constructor: Initializes database, loads data
- `LoadData()` - Private: Loads attendance records with employees
- `AddAttendance()` - Command: Creates new attendance record
- `CheckIn()` - Command: Records check-in time for selected employee (today)
- `CheckOut()` - Command: Records check-out time, calculates hours worked
- `SaveAttendance()` - Command: Saves manual attendance record
- `DeleteAttendance()` - Command: Deletes attendance record
- `CancelEdit()` - Command: Cancels editing
- `OnSelectedDateChanged(DateTime value)` - Partial: Reloads data when date changes

**Properties:**
- `Attendances` - Collection of attendance records
- `Employees` - Collection of employees
- `SelectedAttendance` - Currently selected attendance record
- `SelectedEmployee` - Currently selected employee
- `SelectedDate` - Date filter
- `IsEditMode` - Whether edit form is visible

#### **Attendance Model (Core/Attendance.cs):**
**Properties:**
- `Id` - Primary key
- `EmployeeId` - Foreign key to Employee
- `Date` - Attendance date
- `CheckIn` - Check-in time (nullable)
- `CheckOut` - Check-out time (nullable)
- `HoursWorked` - Calculated hours (double)
- `Employee` - Navigation property

---

### 6️⃣ **LEAVE MANAGEMENT PAGE** 🏖️

#### **Files Used:**
- `Views/LeaveView.xaml` - UI Layout
- `Views/LeaveView.xaml.cs` - Code-behind (double-click handler)
- `ViewModels/LeaveViewModel.cs` - Business Logic
- `Core/LeaveRequest.cs` - LeaveRequest Model
- `Core/Employee.cs` - Employee Model
- `Data/HRDbContext.cs` - Database Access

#### **How It Works:**
1. **View Requests:** DataGrid shows all leave requests
2. **Add:** Creates new leave request, opens edit form
3. **Edit:** Double-click or select → Edit button → opens form
4. **Approve/Reject:** Quick status update buttons
5. **Save:** Validates dates → saves → refreshes
6. **Delete:** Removes leave request

#### **Functions in LeaveViewModel.cs:**
- `LeaveViewModel()` - Constructor: Initializes database, loads data
- `LoadData()` - Private: Loads leave requests with employees
- `AddLeaveRequest()` - Command: Creates new leave request
- `EditLeaveRequest()` - Command: Loads clean copy for editing (avoids EF tracking)
- `ApproveLeave()` - Command: Sets status to Approved
- `RejectLeave()` - Command: Sets status to Rejected
- `SaveLeaveRequest()` - Command: Validates and saves leave request
- `DeleteLeaveRequest()` - Command: Deletes leave request
- `CancelEdit()` - Command: Cancels editing

**Properties:**
- `LeaveRequests` - Collection of leave requests
- `Employees` - Collection of employees
- `SelectedLeaveRequest` - Currently selected leave request
- `IsEditMode` - Whether edit form is visible

#### **LeaveRequest Model (Core/LeaveRequest.cs):**
**Properties:**
- `Id` - Primary key
- `EmployeeId` - Foreign key to Employee
- `StartDate`, `EndDate` - Leave period
- `LeaveType` - Enum: Vacation, Sick, Personal, Maternity, Paternity, Unpaid
- `Status` - Enum: Pending, Approved, Rejected
- `Reason` - Optional reason text
- `Employee` - Navigation property
- `DaysRequested` - Computed property: calculates days

---

### 7️⃣ **PAYROLL PROCESSING PAGE** 💰

#### **Files Used:**
- `Views/PayrollView.xaml` - UI Layout
- `Views/PayrollView.xaml.cs` - Code-behind (double-click, text changed handlers)
- `ViewModels/PayrollViewModel.cs` - Business Logic
- `Core/Payroll.cs` - Payroll Model
- `Core/Employee.cs` - Employee Model
- `Core/Attendance.cs` - Attendance Model (for bonus calculation)
- `Data/HRDbContext.cs` - Database Access

#### **How It Works:**
1. **Select Employee & Period:** Choose employee, month, year
2. **Calculate Payroll:** 
   - Gets base salary from employee
   - Calculates deductions (Tax Rate % + Insurance Rate %)
   - Calculates bonuses (attendance days × Bonus per Day)
   - Calculates net salary
   - Saves to database
3. **Filter:** Shows payrolls for selected month/year
4. **Edit:** Select payroll → Edit button → modify values
5. **Recalculate:** Recalculates based on current settings
6. **Save/Delete:** Saves changes or deletes record

#### **Functions in PayrollViewModel.cs:**
- `PayrollViewModel()` - Constructor: Initializes database, loads data
- `LoadData()` - Private: Loads payrolls with employees
- `FilterPayrolls()` - Private: Filters payrolls by month/year
- `CalculatePayroll()` - Command: Calculates and creates new payroll
- `EditPayroll()` - Command: Loads payroll for editing (clean copy)
- `RecalculatePayroll()` - Command: Recalculates deductions/bonuses/net salary
- `SavePayroll()` - Command: Validates and saves payroll
- `DeletePayroll()` - Command: Deletes payroll record
- `CancelEdit()` - Command: Cancels editing

**Properties:**
- `Payrolls` - All payroll records
- `FilteredPayrolls` - Filtered payroll records (by month/year)
- `Employees` - Collection of employees
- `SelectedPayroll` - Currently selected payroll
- `SelectedEmployee` - Selected employee for calculation
- `SelectedMonth`, `SelectedYear` - Period selection
- `FilterByMonthYear` - Toggle for filtering
- `IsEditMode` - Whether edit form is visible
- `TaxRate` - Tax percentage (default: 10%)
- `InsuranceRate` - Insurance percentage (default: 5%)
- `BonusPerDay` - Bonus amount per full workday (default: $50)

#### **Payroll Model (Core/Payroll.cs):**
**Properties:**
- `Id` - Primary key
- `EmployeeId` - Foreign key to Employee
- `Month`, `Year` - Payroll period
- `BaseSalary` - Employee's base salary
- `Deductions` - Total deductions (tax + insurance)
- `Bonuses` - Total bonuses earned
- `NetSalary` - Final salary (BaseSalary - Deductions + Bonuses)
- `Employee` - Navigation property

---

### 8️⃣ **USER MANAGEMENT PAGE** 👤 (Admin Only)

#### **Files Used:**
- `Views/UserManagementView.xaml` - UI Layout
- `Views/UserManagementView.xaml.cs` - Code-behind
- `ViewModels/UserManagementViewModel.cs` - Business Logic
- `Core/User.cs` - User Model
- `Data/AuthenticationService.cs` - Password hashing
- `Data/HRDbContext.cs` - Database Access

#### **How It Works:**
1. **View Users:** DataGrid shows all system users
2. **Select User:** Click Edit button → loads user into form
3. **Add:** Fill form → creates new user with hashed password
4. **Update:** Modify fields → updates user (password optional)
5. **Delete:** Removes user from system
6. **Clear:** Clears form

#### **Functions in UserManagementViewModel.cs:**
- `UserManagementViewModel()` - Constructor: Initializes database, loads users
- `LoadUsers()` - Private: Loads all users from database
- `AddUser()` - Command: Creates new user with validation
- `UpdateUser()` - Command: Updates existing user
- `DeleteUser()` - Command: Deletes user
- `SelectUser(User user)` - Command: Loads user into form for editing
- `ClearForm()` - Command: Clears all form fields

**Properties:**
- `Users` - Collection of all users
- `SelectedUser` - Currently selected user
- `Username`, `Password`, `FullName`, `Email` - Form fields
- `SelectedRole` - User role (Admin/HR)
- `IsActive` - User active status
- `UserRoles` - Array of available roles

#### **User Model (Core/User.cs):**
**Properties:**
- `Id` - Primary key
- `Username` - Unique username
- `PasswordHash` - SHA-256 hashed password
- `FullName` - User's full name
- `Email` - Unique email
- `Role` - Enum: Admin or HR
- `IsActive` - Account status
- `CreatedDate` - Account creation date
- `LastLoginDate` - Last login timestamp

---

## 📁 File Roles and Functions

### **CORE MODELS** (HRManagementApp.Core/)

#### **Employee.cs**
**Role:** Domain model representing an employee
**Properties:** Id, FirstName, LastName, Email, Phone, HireDate, DepartmentId, Position, Salary
**Navigation Properties:** Department, Attendances, LeaveRequests, Payrolls
**Computed Property:** `FullName` - Returns "FirstName LastName"

#### **Department.cs**
**Role:** Domain model representing a department
**Properties:** Id, Name, Description, ManagerId
**Navigation Properties:** Manager (Employee), Employees collection

#### **Attendance.cs**
**Role:** Domain model representing attendance record
**Properties:** Id, EmployeeId, Date, CheckIn, CheckOut, HoursWorked
**Navigation Property:** Employee

#### **LeaveRequest.cs**
**Role:** Domain model representing leave request
**Properties:** Id, EmployeeId, StartDate, EndDate, LeaveType, Status, Reason
**Enums:** `LeaveType` (Vacation, Sick, Personal, Maternity, Paternity, Unpaid)
**Enums:** `LeaveStatus` (Pending, Approved, Rejected)
**Computed Property:** `DaysRequested` - Calculates days between start and end

#### **Payroll.cs**
**Role:** Domain model representing payroll record
**Properties:** Id, EmployeeId, Month, Year, BaseSalary, Deductions, Bonuses, NetSalary
**Navigation Property:** Employee

#### **User.cs**
**Role:** Domain model representing system user
**Properties:** Id, Username, PasswordHash, FullName, Email, Role, IsActive, CreatedDate, LastLoginDate
**Enum:** `UserRole` (Admin, HR)

---

### **DATA LAYER** (HRManagementApp.Data/)

#### **HRDbContext.cs**
**Role:** Entity Framework database context - manages database connections and entity configurations

**Functions:**
- `OnConfiguring(DbContextOptionsBuilder)` - Configures SQLite connection string
- `OnModelCreating(ModelBuilder)` - Configures entity relationships, constraints, indexes

**DbSets (Database Tables):**
- `Users` - System users table
- `Employees` - Employees table
- `Departments` - Departments table
- `Attendances` - Attendance records table
- `LeaveRequests` - Leave requests table
- `Payrolls` - Payroll records table

**Entity Configurations:**
- User: Unique Username, Unique Email, MaxLength constraints
- Employee: Foreign key to Department (SetNull on delete)
- Department: Foreign key to Manager (SetNull on delete)
- Attendance: Foreign key to Employee (Cascade delete)
- LeaveRequest: Foreign key to Employee (Cascade delete)
- Payroll: Foreign key to Employee (Cascade delete), Decimal precision

#### **AuthenticationService.cs**
**Role:** Handles user authentication and password management

**Functions:**
- `Login(string username, string password)` - Authenticates user, returns User or null
- `HashPassword(string password)` - Static: Hashes password using SHA-256 + Base64
- `CreateUser(...)` - Creates new user account with validation
- `ChangePassword(int userId, string oldPassword, string newPassword)` - Changes user password

#### **DatabaseInitializer.cs**
**Role:** Initializes database and seeds initial data

**Functions:**
- `Initialize(HRDbContext context)` - Static: Creates database, seeds initial data if empty

**Seed Data:**
- 2 default users (admin/admin123, hr/hr123)
- 4 departments (IT, HR, Finance, Sales)
- 3 sample employees

---

### **VIEWMODELS** (HRManagementApp/ViewModels/)

All ViewModels follow MVVM pattern with:
- `[ObservableProperty]` - Auto-generates properties with change notifications
- `[RelayCommand]` - Auto-generates command properties
- Constructor initializes database context
- `LoadData()` method loads data from database
- CRUD operations (Create, Read, Update, Delete)

**Common Pattern:**
1. Initialize database context in constructor
2. Load data on startup
3. Commands handle user actions
4. Validation before save
5. Error handling with try-catch
6. Success/error messages

---

### **VIEWS** (HRManagementApp/Views/)

All Views follow MVVM pattern:
- XAML defines UI layout
- DataContext set to ViewModel
- Data binding connects UI to ViewModel properties
- Commands bind to ViewModel methods
- Converters handle value transformations

**Common Elements:**
- DataGrid for displaying lists
- Edit forms with ScrollViewer for scrolling
- Buttons with Command bindings
- Validation error messages

---

### **CONVERTERS** (HRManagementApp/Converters/)

#### **BoolToVisibilityConverter.cs**
**Role:** Converts boolean/string values to UI Visibility

**Functions:**
- `Convert(object value, ...)` - Converts bool/string to Visibility.Visible/Collapsed
- `ConvertBack(object value, ...)` - Converts Visibility back to bool

**Usage:** Hides/shows UI elements based on boolean properties (e.g., IsEditMode)

---

### **APPLICATION ENTRY POINT**

#### **App.xaml.cs**
**Role:** Application startup logic

**Functions:**
- `OnStartup(StartupEventArgs)` - Called when app starts
  - Initializes database
  - Seeds initial data
  - Shows LoginWindow

---

## 🔄 Data Flow

### **Typical CRUD Operation Flow:**

1. **User Action** → Button Click / DataGrid Selection
2. **Command Execution** → ViewModel method called
3. **Validation** → Check input validity
4. **Database Operation** → EF Core query/update
5. **Save Changes** → `SaveChanges()` commits to database
6. **Reload Data** → Refresh collections from database
7. **UI Update** → ObservableCollection notifies UI automatically
8. **User Feedback** → Success/error message

### **Entity Framework Tracking Pattern:**

**Problem:** Entities loaded with `.Include()` have navigation properties that cause tracking conflicts

**Solution:** 
- For **Updates:** Reload entity using `Find()` → Update scalar properties → Save
- For **Deletes:** Reload entity using `Find()` → Remove → Save
- For **New Records:** Create clean entity without navigation properties → Add → Save

---

## 🎯 Key Design Patterns Used

1. **MVVM Pattern** - Separation of UI and business logic
2. **Repository Pattern** - Database access through DbContext
3. **Command Pattern** - RelayCommand for button actions
4. **Observer Pattern** - ObservableCollection for UI updates
5. **Singleton Pattern** - Views created once in MainViewModel
6. **Factory Pattern** - DatabaseInitializer creates seed data

---

## 🔐 Security Features

- **Password Hashing:** SHA-256 (no salt - limitation)
- **Role-Based Access:** Admin vs HR permissions
- **Input Validation:** Email format, required fields, duplicates
- **SQL Injection Prevention:** Entity Framework parameterized queries
- **User Activation:** IsActive flag for account control

---

## 📊 Database Relationships

```
User (1) ──┐
           │
Employee (N) ── (1) Department (1) ── (N) Employee (Manager)
   │
   ├── (N) Attendance
   ├── (N) LeaveRequest
   └── (N) Payroll
```

**Cascade Rules:**
- Delete Employee → Cascade delete Attendances, LeaveRequests, Payrolls
- Delete Department → SetNull Employee.DepartmentId
- Delete Employee (Manager) → SetNull Department.ManagerId

---

This documentation covers all pages, files, roles, and functions in the HR Management Application!

